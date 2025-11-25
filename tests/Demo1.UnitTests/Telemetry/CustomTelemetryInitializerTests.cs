using Demo1.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;

namespace Demo1.UnitTests.Telemetry;

public class CustomTelemetryInitializerTests
{
    [Fact]
    public void Initialize_AddsApplicationName_WhenMissing()
    {
        var telemetry = new TraceTelemetry("test");
        var initializer = new CustomTelemetryInitializer("Demo1App");

        initializer.Initialize(telemetry);

        Assert.True(telemetry.Context.GlobalProperties.TryGetValue("ApplicationName", out var value));
        Assert.Equal("Demo1App", value);
    }

    [Fact]
    public void Initialize_DoesNotOverwriteExistingApplicationName()
    {
        var telemetry = new TraceTelemetry("test");
        telemetry.Context.GlobalProperties["ApplicationName"] = "Existing";
        var initializer = new CustomTelemetryInitializer("Demo1App");

        initializer.Initialize(telemetry);

        Assert.Equal("Existing", telemetry.Context.GlobalProperties["ApplicationName"]);
    }
}
