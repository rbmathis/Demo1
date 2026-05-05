using Demo1.Models;

namespace Demo1.Services;

/// <summary>
/// Provides application health metadata such as version and uptime.
/// </summary>
public class HealthService : IHealthService
{
    private readonly string _version;
    private readonly string _environment;
    private readonly DateTime _startTimeUtc;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthService"/> class.
    /// </summary>
    /// <param name="webHostEnvironment">The hosting environment.</param>
    /// <param name="configuration">The application configuration.</param>
    public HealthService(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
    {
        _startTimeUtc = DateTime.UtcNow;
        _environment = webHostEnvironment.EnvironmentName;

        var versionFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "VERSION");
        if (File.Exists(versionFilePath))
        {
            _version = File.ReadAllText(versionFilePath).Trim();
        }
        else
        {
            _version = configuration["Application:Version"] ?? "unknown";
        }
    }

    /// <summary>
    /// Gets current application health information.
    /// </summary>
    /// <returns>The current <see cref="HealthInfo"/> payload.</returns>
    public HealthInfo GetHealthInfo()
    {
        var timestamp = DateTime.UtcNow;
        var uptimeInSeconds = (timestamp - _startTimeUtc).TotalSeconds;

        return new HealthInfo(
            _version,
            uptimeInSeconds,
            timestamp,
            _environment);
    }
}
