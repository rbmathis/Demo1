using Demo1.Data;
using Demo1.Models;
using Demo1.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="AchievementService"/> verifying trophy case queries and progress calculation.
/// </summary>
public class AchievementServiceTests
{
    /// <summary>
    /// Creates an in-memory SQLite AchievementDbContext with seed data for testing.
    /// </summary>
    private static AchievementDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AchievementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AchievementDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetTrophyCaseAsync_ReturnsAllAchievements_WhenNoProgress()
    {
        // Arrange
        using var db = CreateInMemoryContext();
        var logger = new Mock<ILogger<AchievementService>>();
        var service = new AchievementService(db, logger.Object);

        // Act
        var result = await service.GetTrophyCaseAsync("test-session");

        // Assert
        Assert.Equal(6, result.Achievements.Count);
        Assert.Equal(0, result.TotalEarned);
        Assert.Equal(6, result.TotalAvailable);
        Assert.All(result.Achievements, a => Assert.False(a.IsEarned));
    }

    [Fact]
    public async Task GetTrophyCaseAsync_ShowsCorrectProgress_ForExplorer()
    {
        // Arrange
        using var db = CreateInMemoryContext();
        var logger = new Mock<ILogger<AchievementService>>();
        var service = new AchievementService(db, logger.Object);
        var sessionId = "test-session";

        // Add 3 page visit events to different paths
        db.AchievementEvents.AddRange(
            new AchievementEvent { SessionId = sessionId, RequestPath = "/", HttpMethod = "GET", StatusCode = 200, EventType = "PageVisit" },
            new AchievementEvent { SessionId = sessionId, RequestPath = "/Home/Privacy", HttpMethod = "GET", StatusCode = 200, EventType = "PageVisit" },
            new AchievementEvent { SessionId = sessionId, RequestPath = "/Home/AboutUs", HttpMethod = "GET", StatusCode = 200, EventType = "PageVisit" }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetTrophyCaseAsync(sessionId);

        // Assert
        var explorer = result.Achievements.First(a => a.Achievement.Name == "Explorer");
        Assert.Equal(3, explorer.CurrentProgress);
        Assert.Equal(5, explorer.Target);
        Assert.False(explorer.IsEarned);
        Assert.Equal(60, explorer.ProgressPercentage);
    }

    [Fact]
    public async Task GetTrophyCaseAsync_MarksAchievementEarned_WhenAlreadyAwarded()
    {
        // Arrange
        using var db = CreateInMemoryContext();
        var logger = new Mock<ILogger<AchievementService>>();
        var service = new AchievementService(db, logger.Object);
        var sessionId = "test-session";

        // Insert a UserAchievement for Explorer
        var earnedAt = DateTime.UtcNow;
        db.UserAchievements.Add(new UserAchievement
        {
            SessionId = sessionId,
            AchievementId = 1, // Explorer
            EarnedAt = earnedAt
        });
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetTrophyCaseAsync(sessionId);

        // Assert
        var explorer = result.Achievements.First(a => a.Achievement.Name == "Explorer");
        Assert.True(explorer.IsEarned);
        Assert.Equal(earnedAt, explorer.EarnedAt);
        Assert.Equal(1, result.TotalEarned);
    }

    [Fact]
    public async Task GetEarnedAchievementsAsync_ReturnsOnlyEarned()
    {
        // Arrange
        using var db = CreateInMemoryContext();
        var logger = new Mock<ILogger<AchievementService>>();
        var service = new AchievementService(db, logger.Object);
        var sessionId = "test-session";

        db.UserAchievements.AddRange(
            new UserAchievement { SessionId = sessionId, AchievementId = 1 },
            new UserAchievement { SessionId = sessionId, AchievementId = 2 }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetEarnedAchievementsAsync(sessionId);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetEarnedCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        using var db = CreateInMemoryContext();
        var logger = new Mock<ILogger<AchievementService>>();
        var service = new AchievementService(db, logger.Object);
        var sessionId = "test-session";

        db.UserAchievements.AddRange(
            new UserAchievement { SessionId = sessionId, AchievementId = 1 },
            new UserAchievement { SessionId = sessionId, AchievementId = 3 },
            new UserAchievement { SessionId = sessionId, AchievementId = 5 }
        );
        await db.SaveChangesAsync();

        // Act
        var count = await service.GetEarnedCountAsync(sessionId);

        // Assert
        Assert.Equal(3, count);
    }
}
