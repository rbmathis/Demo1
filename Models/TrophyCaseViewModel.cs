namespace Demo1.Models;

/// <summary>
/// View model for the Trophy Case page, displaying all achievements with progress.
/// </summary>
public class TrophyCaseViewModel
{
    /// <summary>
    /// Gets or sets the list of achievement progress entries.
    /// </summary>
    public List<AchievementProgress> Achievements { get; set; } = new();

    /// <summary>
    /// Gets or sets the total number of achievements earned by this user.
    /// </summary>
    public int TotalEarned { get; set; }

    /// <summary>
    /// Gets or sets the total number of achievements available.
    /// </summary>
    public int TotalAvailable { get; set; }
}

/// <summary>
/// Represents the progress toward a single achievement for display in the trophy case.
/// </summary>
public class AchievementProgress
{
    /// <summary>
    /// Gets or sets the achievement definition.
    /// </summary>
    public Achievement Achievement { get; set; } = null!;

    /// <summary>
    /// Gets or sets whether this achievement has been earned.
    /// </summary>
    public bool IsEarned { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this achievement was earned, if applicable.
    /// </summary>
    public DateTime? EarnedAt { get; set; }

    /// <summary>
    /// Gets or sets the current progress count toward the achievement target.
    /// </summary>
    public int CurrentProgress { get; set; }

    /// <summary>
    /// Gets or sets the target count needed to earn the achievement.
    /// </summary>
    public int Target { get; set; }

    /// <summary>
    /// Gets the progress percentage (0-100), capped at 100.
    /// </summary>
    public double ProgressPercentage => Target > 0
        ? Math.Min((double)CurrentProgress / Target * 100, 100)
        : 0;
}

/// <summary>
/// View model for the Synchronous Anti-Pattern page, showing timing comparison data.
/// </summary>
public class SynchronousAntiPatternViewModel
{
    /// <summary>
    /// Gets or sets the list of achievement progress entries computed synchronously.
    /// </summary>
    public List<AchievementProgress> Achievements { get; set; } = new();

    /// <summary>
    /// Gets or sets the elapsed time in milliseconds for the synchronous processing.
    /// </summary>
    public long ElapsedMs { get; set; }

    /// <summary>
    /// Gets or sets the total number of events queried synchronously.
    /// </summary>
    public int EventsQueried { get; set; }

    /// <summary>
    /// Gets or sets the total number of rules evaluated synchronously.
    /// </summary>
    public int RulesEvaluated { get; set; }
}
