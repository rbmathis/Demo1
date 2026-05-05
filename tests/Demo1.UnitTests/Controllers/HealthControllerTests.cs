using Demo1.Controllers;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace Demo1.UnitTests.Controllers;

public class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsOkWithHealthInfo()
    {
        var expected = new HealthInfo("1.2.3", 12, DateTime.UtcNow, "Testing");
        var healthService = new Mock<IHealthService>();
        healthService.Setup(x => x.GetHealthInfo()).Returns(expected);
        var controller = new HealthController(healthService.Object);

        var result = controller.Get();

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<HealthInfo>(ok.Value);
        Assert.Equal(expected, payload);
    }

    [Fact]
    public void Get_ReturnsVersion_FromService()
    {
        const string expectedVersion = "9.9.9-test";
        var healthInfo = new HealthInfo(expectedVersion, 5, DateTime.UtcNow, "Testing");
        var healthService = new Mock<IHealthService>();
        healthService.Setup(x => x.GetHealthInfo()).Returns(healthInfo);
        var controller = new HealthController(healthService.Object);

        var result = controller.Get();

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<HealthInfo>(ok.Value);
        Assert.Equal(expectedVersion, payload.Version);
    }

    [Fact]
    public void Get_ReturnsUptimeSeconds_Positive()
    {
        var healthInfo = new HealthInfo("1.0.0", 42, DateTime.UtcNow, "Testing");
        var healthService = new Mock<IHealthService>();
        healthService.Setup(x => x.GetHealthInfo()).Returns(healthInfo);
        var controller = new HealthController(healthService.Object);

        var result = controller.Get();

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<HealthInfo>(ok.Value);
        Assert.True(payload.UptimeSeconds > 0);
    }

    [Fact]
    public void Get_ReturnsTimestamp_IsUtc()
    {
        var healthInfo = new HealthInfo("1.0.0", 1, DateTime.UtcNow, "Testing");
        var healthService = new Mock<IHealthService>();
        healthService.Setup(x => x.GetHealthInfo()).Returns(healthInfo);
        var controller = new HealthController(healthService.Object);

        var result = controller.Get();

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<HealthInfo>(ok.Value);
        Assert.Equal(DateTimeKind.Utc, payload.Timestamp.Kind);
    }

    [Fact]
    public void HealthService_ReadsVersionFromFile()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), $"demo1-health-{Guid.NewGuid():N}");
        Directory.CreateDirectory(contentRoot);
        const string versionFromFile = "2.5.0-test";
        File.WriteAllText(Path.Combine(contentRoot, "VERSION"), $"{versionFromFile}{Environment.NewLine}");

        try
        {
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(x => x.ContentRootPath).Returns(contentRoot);
            environment.SetupGet(x => x.EnvironmentName).Returns("Testing");
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Application:Version"] = "fallback" })
                .Build();

            var service = new HealthService(environment.Object, configuration);

            var result = service.GetHealthInfo();

            Assert.Equal(versionFromFile, result.Version);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public void HealthService_UptimeSeconds_IncreasesOverTime()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), $"demo1-health-{Guid.NewGuid():N}");
        Directory.CreateDirectory(contentRoot);

        try
        {
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(x => x.ContentRootPath).Returns(contentRoot);
            environment.SetupGet(x => x.EnvironmentName).Returns("Testing");
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Application:Version"] = "1.0.0" })
                .Build();

            var service = new HealthService(environment.Object, configuration);
            var first = service.GetHealthInfo();
            var second = first;

            for (var i = 0; i < 5 && second.UptimeSeconds <= first.UptimeSeconds; i++)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(1100));
                second = service.GetHealthInfo();
            }

            Assert.True(second.UptimeSeconds > first.UptimeSeconds);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }
}
