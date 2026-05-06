namespace Demo1.Models;

/// <summary>
/// Configuration options for rate limiting behavior.
/// Binds to the "RateLimiting" section in appsettings.json.
/// </summary>
public class RateLimitingOptions
{
    /// <summary>
    /// The configuration section name for rate limiting options.
    /// </summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Maximum number of requests permitted within the time window.
    /// Default is 100 requests.
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// The time window duration in seconds for rate limiting.
    /// Default is 60 seconds (1 minute).
    /// </summary>
    public int WindowInSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum number of requests that can be queued when the limit is reached.
    /// Default is 0 (no queuing).
    /// </summary>
    public int QueueLimit { get; set; } = 0;
}
