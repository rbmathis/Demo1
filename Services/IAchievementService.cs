using Demo1.Data;
using Demo1.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Services;

/// <summary>
/// Provides achievement data retrieval and progress calculation for the trophy case.
/// </summary>
public interface IAchievementService
{
    /// <summary>
    /// Gets the full trophy case with progress for all achievements for a given session.
    /// </summary>
    /// <param name="sessionId">The user's session identifier.</param>
    /// <returns>A trophy case view model with achievement progress.</returns>
    Task<TrophyCaseViewModel> GetTrophyCaseAsync(string sessionId);

    /// <summary>
    /// Gets only the achievements that have been earned by a given session.
    /// </summary>
    /// <param name="sessionId">The user's session identifier.</param>
    /// <returns>A list of earned user achievements with their definitions.</returns>
    Task<List<UserAchievement>> GetEarnedAchievementsAsync(string sessionId);

    /// <summary>
    /// Gets the count of achievements earned by a given session.
    /// </summary>
    /// <param name="sessionId">The user's session identifier.</param>
    /// <returns>The number of earned achievements.</returns>
    Task<int> GetEarnedCountAsync(string sessionId);
}

/// <summary>
/// Implementation of <see cref="IAchievementService"/> that queries the achievement database
/// for badge definitions, earned status, and progress calculation.
/// </summary>
public class AchievementService : IAchievementService
{
    private readonly AchievementDbContext _db;
    private readonly ILogger<AchievementService> _logger;

    /// <summary>
    /// The known list of core site pages used for the "Completionist" achievement.
    /// </summary>
    private static readonly string[] CorePages = new[]
    {
        "/",
        "/Home/Privacy",
        "/Home/AboutUs",
        "/Performance/Dashboard",
        "/SecurityLab",
        "/Achievement/TrophyCase",
        "/Home/GodObjectProfile",
        "/Home/RawSqlSearch"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementService"/> class.
    /// </summary>
    /// <param name="db">The achievement database context.</param>
    /// <param name="logger">The logger instance.</param>
    public AchievementService(AchievementDbContext db, ILogger<AchievementService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TrophyCaseViewModel> GetTrophyCaseAsync(string sessionId)
    {
        var achievements = await _db.Achievements.ToListAsync();
        var earned = await _db.UserAchievements
            .Where(ua => ua.SessionId == sessionId)
            .ToListAsync();
        var events = await _db.AchievementEvents
            .Where(e => e.SessionId == sessionId)
            .ToListAsync();

        var progressList = new List<AchievementProgress>();

        foreach (var achievement in achievements)
        {
            var earnedRecord = earned.FirstOrDefault(e => e.AchievementId == achievement.Id);
            var currentProgress = CalculateProgress(achievement, events);

            progressList.Add(new AchievementProgress
            {
                Achievement = achievement,
                IsEarned = earnedRecord != null,
                EarnedAt = earnedRecord?.EarnedAt,
                CurrentProgress = currentProgress,
                Target = achievement.Target
            });
        }

        return new TrophyCaseViewModel
        {
            Achievements = progressList,
            TotalEarned = earned.Count,
            TotalAvailable = achievements.Count
        };
    }

    /// <inheritdoc />
    public async Task<List<UserAchievement>> GetEarnedAchievementsAsync(string sessionId)
    {
        return await _db.UserAchievements
            .Include(ua => ua.Achievement)
            .Where(ua => ua.SessionId == sessionId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> GetEarnedCountAsync(string sessionId)
    {
        return await _db.UserAchievements
            .CountAsync(ua => ua.SessionId == sessionId);
    }

    /// <summary>
    /// Calculates the current progress for a specific achievement based on event history.
    /// </summary>
    /// <param name="achievement">The achievement to calculate progress for.</param>
    /// <param name="events">The list of events for the current session.</param>
    /// <returns>The current progress count.</returns>
    public static int CalculateProgress(Achievement achievement, List<AchievementEvent> events)
    {
        return achievement.TriggerType switch
        {
            "PageVisitCount" => events
                .Where(e => e.EventType == "PageVisit")
                .Select(e => e.RequestPath)
                .Distinct()
                .Count(),

            "RateLimited" => events
                .Count(e => e.StatusCode == 429),

            "SecurityLabXss" => events
                .Count(e => e.EventType == "SecurityLabAttack"
                    && !string.IsNullOrEmpty(achievement.TriggerValue)
                    && e.RequestPath.StartsWith(achievement.TriggerValue, StringComparison.OrdinalIgnoreCase)),

            "SpecificPage" => events
                .Any(e => !string.IsNullOrEmpty(achievement.TriggerValue)
                    && e.RequestPath.Equals(achievement.TriggerValue, StringComparison.OrdinalIgnoreCase))
                ? 1 : 0,

            "ApiCall" => events
                .Any(e => e.RequestPath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
                ? 1 : 0,

            "AllPages" => CorePages
                .Count(page => events.Any(e =>
                    e.RequestPath.Equals(page, StringComparison.OrdinalIgnoreCase))),

            _ => 0
        };
    }
}
