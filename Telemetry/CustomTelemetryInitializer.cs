using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Demo1.Telemetry;

/// <summary>
/// Telemetry initializer that adds custom properties to all telemetry items.
/// </summary>
public class CustomTelemetryInitializer : ITelemetryInitializer
{
    private readonly string _applicationName;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomTelemetryInitializer"/> class.
    /// </summary>
    /// <param name="applicationName">The application name to add to telemetry.</param>
    public CustomTelemetryInitializer(string applicationName)
    {
        _applicationName = applicationName;
    }

    /// <summary>
    /// Initializes properties of the telemetry item.
    /// </summary>
    /// <param name="telemetry">The telemetry item to initialize.</param>
    public void Initialize(ITelemetry telemetry)
    {
        // Add custom properties to all telemetry
        if (!telemetry.Context.GlobalProperties.ContainsKey("ApplicationName"))
        {
            telemetry.Context.GlobalProperties["ApplicationName"] = _applicationName;
        }
    }
}
