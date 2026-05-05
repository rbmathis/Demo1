namespace Demo1.Models;

/// <summary>
/// Represents health status information for the application.
/// </summary>
/// <param name="Version">The application version.</param>
/// <param name="UptimeSeconds">The application uptime in seconds.</param>
/// <param name="Timestamp">The UTC timestamp when the health payload was generated.</param>
/// <param name="Environment">The current hosting environment name.</param>
public record HealthInfo(
    string Version,
    double UptimeSeconds,
    DateTime Timestamp,
    string Environment);
