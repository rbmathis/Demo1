namespace Demo1.Services;

/// <summary>
/// Provides application uptime information.
/// </summary>
public interface IUptimeService
{
    /// <summary>
    /// Gets the elapsed time since the application started.
    /// </summary>
    /// <returns>The application uptime.</returns>
    TimeSpan GetUptime();
}

/// <summary>
/// Default implementation of <see cref="IUptimeService"/>.
/// </summary>
public class UptimeService : IUptimeService
{
    private readonly DateTime _startedAtUtc = DateTime.UtcNow;

    /// <summary>
    /// Gets the elapsed time since the service was created.
    /// </summary>
    /// <returns>The application uptime.</returns>
    public TimeSpan GetUptime()
    {
        return DateTime.UtcNow - _startedAtUtc;
    }
}
