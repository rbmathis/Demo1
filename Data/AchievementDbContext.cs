using Demo1.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Data;

/// <summary>
/// Entity Framework Core database context for the Achievement system.
/// Manages Achievement definitions, UserAchievement records, and AchievementEvent history.
/// </summary>
public class AchievementDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public AchievementDbContext(DbContextOptions<AchievementDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the achievement badge definitions.
    /// </summary>
    public DbSet<Achievement> Achievements => Set<Achievement>();

    /// <summary>
    /// Gets or sets the earned achievement records.
    /// </summary>
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

    /// <summary>
    /// Gets or sets the achievement event history.
    /// </summary>
    public DbSet<AchievementEvent> AchievementEvents => Set<AchievementEvent>();

    /// <summary>
    /// Configures the entity model, including indexes, constraints, and seed data.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure entities.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure UserAchievement indexes and constraints
        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => new { e.SessionId, e.AchievementId }).IsUnique();
        });

        // Configure AchievementEvent indexes
        modelBuilder.Entity<AchievementEvent>(entity =>
        {
            entity.HasIndex(e => e.SessionId);
        });

        // Seed achievement definitions
        modelBuilder.Entity<Achievement>().HasData(
            new Achievement
            {
                Id = 1,
                Name = "Explorer",
                Description = "Visit 5 different pages",
                Icon = "\U0001F9ED",  // 🧭
                Target = 5,
                TriggerType = "PageVisitCount"
            },
            new Achievement
            {
                Id = 2,
                Name = "Speed Demon",
                Description = "Trigger rate limiting",
                Icon = "\u26A1",  // ⚡
                Target = 1,
                TriggerType = "RateLimited"
            },
            new Achievement
            {
                Id = 3,
                Name = "White Hat",
                Description = "Find an XSS vector in Security Lab",
                Icon = "\U0001F3A9",  // 🎩
                Target = 1,
                TriggerType = "SecurityLabXss",
                TriggerValue = "/SecurityLab/Attack"
            },
            new Achievement
            {
                Id = 4,
                Name = "Benchmarker",
                Description = "View the Performance Dashboard",
                Icon = "\U0001F4CA",  // 📊
                Target = 1,
                TriggerType = "SpecificPage",
                TriggerValue = "/Performance/Dashboard"
            },
            new Achievement
            {
                Id = 5,
                Name = "API Curious",
                Description = "Call any API endpoint",
                Icon = "\U0001F50E",  // 🔎
                Target = 1,
                TriggerType = "ApiCall",
                TriggerValue = "/api/"
            },
            new Achievement
            {
                Id = 6,
                Name = "Completionist",
                Description = "Visit every page on the site",
                Icon = "\U0001F3C5",  // 🏅
                Target = 8,
                TriggerType = "AllPages"
            }
        );
    }
}
