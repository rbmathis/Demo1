using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo1.Models;

/// <summary>
/// Represents an achievement badge definition that users can earn.
/// Seeded in the database with predefined triggers and targets.
/// </summary>
public class Achievement
{
    /// <summary>
    /// Gets or sets the unique identifier for this achievement.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the achievement (e.g., "Explorer").
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of how to earn this achievement.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the emoji icon for this achievement (e.g., "🏆").
    /// </summary>
    [Required]
    [StringLength(10)]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the numeric target for progress tracking (e.g., 5 for "visit 5 pages").
    /// </summary>
    public int Target { get; set; }

    /// <summary>
    /// Gets or sets the trigger type that determines how progress is evaluated.
    /// Values: "PageVisitCount", "RateLimited", "SecurityLabXss", "SpecificPage", "ApiCall", "AllPages".
    /// </summary>
    [Required]
    [StringLength(50)]
    public string TriggerType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional parameter for the trigger (e.g., specific path for "SpecificPage" triggers).
    /// </summary>
    [StringLength(200)]
    public string? TriggerValue { get; set; }

    /// <summary>
    /// Gets or sets the collection of user achievements earned for this badge.
    /// </summary>
    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}

/// <summary>
/// Represents a user earning an achievement, tracked by session ID.
/// </summary>
public class UserAchievement
{
    /// <summary>
    /// Gets or sets the unique identifier for this earned achievement record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the session ID of the user who earned this achievement.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the foreign key to the achievement that was earned.
    /// </summary>
    public int AchievementId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this achievement was earned (UTC).
    /// </summary>
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the navigation property to the achievement definition.
    /// </summary>
    public Achievement Achievement { get; set; } = null!;
}

/// <summary>
/// Represents a persisted event record from user activity, used for achievement progress tracking.
/// </summary>
public class AchievementEvent
{
    /// <summary>
    /// Gets or sets the unique identifier for this event.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the session ID of the user who triggered this event.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP request path that triggered this event.
    /// </summary>
    [Required]
    [StringLength(500)]
    public string RequestPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP method used (GET, POST, etc.).
    /// </summary>
    [Required]
    [StringLength(10)]
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP response status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this event occurred (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the classified event type.
    /// Values: "PageVisit", "ApiCall", "RateLimited", "SecurityLabAttack".
    /// </summary>
    [Required]
    [StringLength(50)]
    public string EventType { get; set; } = string.Empty;
}

/// <summary>
/// Lightweight in-memory DTO for the achievement event channel.
/// This is NOT an EF entity — it flows through the Channel&lt;T&gt; pipeline.
/// </summary>
public class AchievementEventMessage
{
    /// <summary>
    /// Gets or sets the session ID of the user.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP request path.
    /// </summary>
    public string RequestPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP method.
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP response status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
