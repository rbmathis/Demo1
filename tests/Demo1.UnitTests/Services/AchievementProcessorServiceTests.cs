using System.Threading.Channels;
using Demo1.Data;
using Demo1.Models;
using Demo1.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="AchievementProcessorService"/> verifying event processing and badge awarding.
/// </summary>
public class AchievementProcessorServiceTests
{
    /// <summary>
    /// Creates an in-memory SQLite AchievementDbContext with seed data for testing.
    /// </summary>
    private static AchievementDbContext CreateInMemoryContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AchievementDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new AchievementDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Helper to process a set of events through the AchievementProcessorService.
    /// </summary>
    private static async Task ProcessEventsAsync(
        List<AchievementEventMessage> messages,
        string dbName)
    {
        var channel = Channel.CreateBounded<AchievementEventMessage>(100);
        var logger = new Mock<ILogger<AchievementProcessorService>>();

        // Build a service collection with a shared in-memory database
        var services = new ServiceCollection();
        services.AddDbContext<AchievementDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        var serviceProvider = services.BuildServiceProvider();

        // Ensure database is created with seed data
        using (var initScope = serviceProvider.CreateScope())
        {
            var initDb = initScope.ServiceProvider.GetRequiredService<AchievementDbContext>();
            initDb.Database.EnsureCreated();
        }

        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var processor = new AchievementProcessorService(channel, scopeFactory, logger.Object);

        // Write messages to channel
        foreach (var msg in messages)
        {
            await channel.Writer.WriteAsync(msg);
        }
        channel.Writer.Complete();

        // Run processor with a timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await processor.StartAsync(cts.Token);

        // Wait for completion — give the background task time to process all messages
        await Task.Delay(2000);
        await processor.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ProcessEvent_AwardsExplorerBadge_WhenFivePagesVisited()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var messages = new List<AchievementEventMessage>();
        for (int i = 1; i <= 5; i++)
        {
            messages.Add(new AchievementEventMessage
            {
                SessionId = "test-session",
                RequestPath = $"/page{i}",
                HttpMethod = "GET",
                StatusCode = 200,
                Timestamp = DateTime.UtcNow
            });
        }

        // Act
        await ProcessEventsAsync(messages, dbName);

        // Assert
        using var db = CreateInMemoryContext(dbName);
        var earned = db.UserAchievements
            .Where(ua => ua.SessionId == "test-session" && ua.AchievementId == 1)
            .ToList();
        Assert.Single(earned);
    }

    [Fact]
    public async Task ProcessEvent_AwardsSpeedDemon_WhenRateLimited()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var messages = new List<AchievementEventMessage>
        {
            new()
            {
                SessionId = "test-session",
                RequestPath = "/some-page",
                HttpMethod = "GET",
                StatusCode = 429,
                Timestamp = DateTime.UtcNow
            }
        };

        // Act
        await ProcessEventsAsync(messages, dbName);

        // Assert
        using var db = CreateInMemoryContext(dbName);
        var earned = db.UserAchievements
            .Where(ua => ua.SessionId == "test-session" && ua.AchievementId == 2) // Speed Demon
            .ToList();
        Assert.Single(earned);
    }

    [Fact]
    public async Task ProcessEvent_AwardsBenchmarker_WhenDashboardVisited()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var messages = new List<AchievementEventMessage>
        {
            new()
            {
                SessionId = "test-session",
                RequestPath = "/Performance/Dashboard",
                HttpMethod = "GET",
                StatusCode = 200,
                Timestamp = DateTime.UtcNow
            }
        };

        // Act
        await ProcessEventsAsync(messages, dbName);

        // Assert
        using var db = CreateInMemoryContext(dbName);
        var earned = db.UserAchievements
            .Where(ua => ua.SessionId == "test-session" && ua.AchievementId == 4) // Benchmarker
            .ToList();
        Assert.Single(earned);
    }

    [Fact]
    public async Task ProcessEvent_AwardsApiCurious_WhenApiCalled()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var messages = new List<AchievementEventMessage>
        {
            new()
            {
                SessionId = "test-session",
                RequestPath = "/api/v1/weatherforecast",
                HttpMethod = "GET",
                StatusCode = 200,
                Timestamp = DateTime.UtcNow
            }
        };

        // Act
        await ProcessEventsAsync(messages, dbName);

        // Assert
        using var db = CreateInMemoryContext(dbName);
        var earned = db.UserAchievements
            .Where(ua => ua.SessionId == "test-session" && ua.AchievementId == 5) // API Curious
            .ToList();
        Assert.Single(earned);
    }

    [Fact]
    public async Task ProcessEvent_DoesNotDuplicate_WhenAlreadyEarned()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();

        // First, earn the Speed Demon badge
        var firstMessages = new List<AchievementEventMessage>
        {
            new()
            {
                SessionId = "test-session",
                RequestPath = "/some-page",
                HttpMethod = "GET",
                StatusCode = 429,
                Timestamp = DateTime.UtcNow
            }
        };
        await ProcessEventsAsync(firstMessages, dbName);

        // Now send another 429 event
        var secondMessages = new List<AchievementEventMessage>
        {
            new()
            {
                SessionId = "test-session",
                RequestPath = "/another-page",
                HttpMethod = "GET",
                StatusCode = 429,
                Timestamp = DateTime.UtcNow
            }
        };
        await ProcessEventsAsync(secondMessages, dbName);

        // Assert — still only one Speed Demon badge
        using var db = CreateInMemoryContext(dbName);
        var earned = db.UserAchievements
            .Where(ua => ua.SessionId == "test-session" && ua.AchievementId == 2)
            .ToList();
        Assert.Single(earned);
    }

    [Fact]
    public void ClassifyEvent_ReturnsCorrectTypes()
    {
        // RateLimited
        Assert.Equal("RateLimited", AchievementProcessorService.ClassifyEvent(
            new AchievementEventMessage { StatusCode = 429, RequestPath = "/test" }));

        // SecurityLabAttack
        Assert.Equal("SecurityLabAttack", AchievementProcessorService.ClassifyEvent(
            new AchievementEventMessage { StatusCode = 200, RequestPath = "/SecurityLab/Attack?xss=test" }));

        // ApiCall
        Assert.Equal("ApiCall", AchievementProcessorService.ClassifyEvent(
            new AchievementEventMessage { StatusCode = 200, RequestPath = "/api/v1/weather" }));

        // PageVisit
        Assert.Equal("PageVisit", AchievementProcessorService.ClassifyEvent(
            new AchievementEventMessage { StatusCode = 200, RequestPath = "/Home/Privacy" }));
    }
}
