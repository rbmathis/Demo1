using System.Threading.Channels;
using Demo1.Data;
using Demo1.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Services;

/// <summary>
/// Background service that consumes achievement events from a Channel&lt;AchievementEventMessage&gt;
/// and processes them asynchronously — persisting events, evaluating achievement rules,
/// and awarding badges when criteria are met.
/// </summary>
public class AchievementProcessorService : BackgroundService
{
    private readonly Channel<AchievementEventMessage> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AchievementProcessorService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementProcessorService"/> class.
    /// </summary>
    /// <param name="channel">The bounded channel to read achievement events from.</param>
    /// <param name="scopeFactory">The service scope factory for creating scoped DbContext instances.</param>
    /// <param name="logger">The logger instance.</param>
    public AchievementProcessorService(
        Channel<AchievementEventMessage> channel,
        IServiceScopeFactory scopeFactory,
        ILogger<AchievementProcessorService> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Continuously reads events from the channel and processes them until cancellation is requested.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token signaling shutdown.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AchievementProcessorService started — listening for events");

        try
        {
            await foreach (var message in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessEventAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing achievement event for session {SessionId}", message.SessionId);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("AchievementProcessorService shutting down gracefully");
        }
    }

    /// <summary>
    /// Processes a single achievement event: persists it, evaluates rules, and awards badges.
    /// </summary>
    /// <param name="message">The achievement event message to process.</param>
    private async Task ProcessEventAsync(AchievementEventMessage message)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AchievementDbContext>();

        // Classify the event type
        var eventType = ClassifyEvent(message);

        // Persist the event
        var achievementEvent = new AchievementEvent
        {
            SessionId = message.SessionId,
            RequestPath = message.RequestPath,
            HttpMethod = message.HttpMethod,
            StatusCode = message.StatusCode,
            Timestamp = message.Timestamp,
            EventType = eventType
        };
        db.AchievementEvents.Add(achievementEvent);
        await db.SaveChangesAsync();

        // Load all achievement definitions
        var achievements = await db.Achievements.ToListAsync();

        // Get already-earned achievements for this session
        var earnedIds = await db.UserAchievements
            .Where(ua => ua.SessionId == message.SessionId)
            .Select(ua => ua.AchievementId)
            .ToListAsync();

        // Get all events for this session (including the one just persisted)
        var sessionEvents = await db.AchievementEvents
            .Where(e => e.SessionId == message.SessionId)
            .ToListAsync();

        // Evaluate each unearned achievement
        foreach (var achievement in achievements.Where(a => !earnedIds.Contains(a.Id)))
        {
            var progress = AchievementService.CalculateProgress(achievement, sessionEvents);

            if (progress >= achievement.Target)
            {
                db.UserAchievements.Add(new UserAchievement
                {
                    SessionId = message.SessionId,
                    AchievementId = achievement.Id,
                    EarnedAt = DateTime.UtcNow
                });

                _logger.LogInformation(
                    "Achievement unlocked! Session {SessionId} earned '{AchievementName}' {Icon}",
                    message.SessionId, achievement.Name, achievement.Icon);
            }
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Classifies an event message into a category for achievement tracking.
    /// </summary>
    /// <param name="message">The event message to classify.</param>
    /// <returns>The event type string.</returns>
    public static string ClassifyEvent(AchievementEventMessage message)
    {
        if (message.StatusCode == 429)
            return "RateLimited";

        if (message.RequestPath.StartsWith("/SecurityLab/Attack", StringComparison.OrdinalIgnoreCase))
            return "SecurityLabAttack";

        if (message.RequestPath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            return "ApiCall";

        return "PageVisit";
    }
}
