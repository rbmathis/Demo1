using Demo1.Models;

namespace Demo1.Services;

/// <summary>
/// Provides application health metadata for health endpoints.
/// </summary>
public interface IHealthService
{
    /// <summary>
    /// Gets current application health information.
    /// </summary>
    /// <returns>The current <see cref="HealthInfo"/> payload.</returns>
    HealthInfo GetHealthInfo();
}
