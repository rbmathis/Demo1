using Demo1.Controllers;
using Demo1.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Controllers;

public class HealthControllerTests
{
    [Fact]
    public void Get_Returns_OkObjectResult()
    {
        var contentRootPath = CreateTemporaryContentRoot("1.0.0-test");
        try
        {
            var controller = CreateController(contentRootPath, "Development", TimeSpan.FromMinutes(5));

            var result = controller.Get();

            Assert.IsType<OkObjectResult>(result);
        }
        finally
        {
            Directory.Delete(contentRootPath, recursive: true);
        }
    }

    [Fact]
    public void Get_Response_Includes_Version_Uptime_Timestamp_Environment_Fields()
    {
        var contentRootPath = CreateTemporaryContentRoot("2.0.0-test");
        try
        {
            var controller = CreateController(contentRootPath, "Testing", TimeSpan.FromMinutes(10));
            var before = DateTime.UtcNow;

            var result = controller.Get();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);

            var version = GetPropertyValue(ok.Value!, "version");
            var uptime = GetPropertyValue(ok.Value!, "uptime");
            var timestamp = GetPropertyValue(ok.Value!, "timestamp");
            var environment = GetPropertyValue(ok.Value!, "environment");

            Assert.NotNull(version);
            Assert.NotNull(uptime);
            Assert.NotNull(timestamp);
            Assert.NotNull(environment);
            Assert.IsType<TimeSpan>(uptime);

            var timestampValue = Assert.IsType<DateTime>(timestamp);
            var after = DateTime.UtcNow;
            Assert.InRange(timestampValue, before, after);
        }
        finally
        {
            Directory.Delete(contentRootPath, recursive: true);
        }
    }

    [Fact]
    public void Get_WithVersionFile_Returns_Version_FromContentRoot()
    {
        const string expectedVersion = "9.9.9-test";
        // Include a trailing newline to verify trimmed VERSION file content.
        var contentRootPath = CreateTemporaryContentRoot($"{expectedVersion}{Environment.NewLine}");
        try
        {
            var controller = CreateController(contentRootPath, "Development", TimeSpan.FromSeconds(30));

            var result = controller.Get();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var version = GetPropertyValue(ok.Value!, "version");
            Assert.Equal(expectedVersion, version);
        }
        finally
        {
            Directory.Delete(contentRootPath, recursive: true);
        }
    }

    [Fact]
    public void Get_Returns_Configured_EnvironmentName()
    {
        const string environmentName = "Production";
        var contentRootPath = CreateTemporaryContentRoot("3.1.4");
        try
        {
            var controller = CreateController(contentRootPath, environmentName, TimeSpan.FromMinutes(1));

            var result = controller.Get();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            var environment = GetPropertyValue(ok.Value!, "environment");
            Assert.Equal(environmentName, environment);
        }
        finally
        {
            Directory.Delete(contentRootPath, recursive: true);
        }
    }

    private static HealthController CreateController(string contentRootPath, string environmentName, TimeSpan uptime)
    {
        var uptimeService = new Mock<IUptimeService>();
        uptimeService.Setup(service => service.GetUptime()).Returns(uptime);

        var hostEnvironment = new Mock<IWebHostEnvironment>();
        hostEnvironment.SetupGet(environment => environment.ContentRootPath).Returns(contentRootPath);
        hostEnvironment.SetupGet(environment => environment.EnvironmentName).Returns(environmentName);

        return new HealthController(
            uptimeService.Object,
            hostEnvironment.Object,
            Mock.Of<ILogger<HealthController>>());
    }

    private static string CreateTemporaryContentRoot(string versionContent)
    {
        var contentRootPath = Path.Combine(Path.GetTempPath(), $"demo1-health-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(contentRootPath);
        File.WriteAllText(Path.Combine(contentRootPath, "VERSION"), versionContent);
        return contentRootPath;
    }

    private static object? GetPropertyValue(object instance, string propertyName)
    {
        return instance.GetType().GetProperty(propertyName)?.GetValue(instance);
    }
}
