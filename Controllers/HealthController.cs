using Demo1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IOFile = System.IO.File;

namespace Demo1.Controllers;

/// <summary>
/// Exposes health and runtime metadata endpoints.
/// </summary>
[AllowAnonymous]
[Route("health")]
public class HealthController : Controller
{
    // Hard upper bound to avoid reading unexpectedly large files.
    private const int MaxVersionFileBytes = 4 * 1024;
    // Version strings should be concise and predictable for monitoring output.
    private const int MaxVersionLength = 64;
    private readonly IUptimeService _uptimeService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<HealthController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthController"/> class.
    /// </summary>
    /// <param name="uptimeService">Service that provides application uptime.</param>
    /// <param name="environment">Host environment details.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public HealthController(
        IUptimeService uptimeService,
        IWebHostEnvironment environment,
        ILogger<HealthController> logger)
    {
        _uptimeService = uptimeService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Returns application health metadata.
    /// </summary>
    /// <returns>HTTP 200 response with version, uptime, timestamp, and environment.</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            version = GetVersionSafe(),
            uptime = _uptimeService.GetUptime(),
            timestamp = DateTime.UtcNow,
            environment = _environment.EnvironmentName,
        });
    }

    private string GetVersionSafe()
    {
        try
        {
            var versionPath = Path.Combine(_environment.ContentRootPath, "VERSION");
            var fullPath = Path.GetFullPath(versionPath);
            var contentRoot = Path.GetFullPath(_environment.ContentRootPath);

            var contentRootWithSeparator = contentRoot.EndsWith(Path.DirectorySeparatorChar)
                ? contentRoot
                : contentRoot + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(contentRootWithSeparator, StringComparison.Ordinal))
            {
                _logger.LogWarning("VERSION file path resolved outside content root.");
                return "unknown";
            }

            var versionFileInfo = new FileInfo(fullPath);
            if (!versionFileInfo.Exists)
            {
                return "unknown";
            }

            if (versionFileInfo.Length > MaxVersionFileBytes)
            {
                _logger.LogWarning("VERSION file exceeded safe size limit.");
                return "unknown";
            }

            using var stream = IOFile.OpenRead(fullPath);
            using var reader = new StreamReader(stream);
            var version = (reader.ReadLine() ?? string.Empty).Trim();
            if (version.Length > MaxVersionLength)
            {
                version = version[..MaxVersionLength];
            }

            return string.IsNullOrWhiteSpace(version) ? "unknown" : version;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read VERSION file.");
            return "unknown";
        }
    }

}
