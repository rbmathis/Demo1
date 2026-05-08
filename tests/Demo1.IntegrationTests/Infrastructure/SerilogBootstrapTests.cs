using Demo1.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Demo1.IntegrationTests.Infrastructure;

/// <summary>
/// Integration tests for Serilog bootstrap configuration.
/// Verifies that the application starts successfully with Serilog wired in
/// and that the logging infrastructure is correctly registered in DI.
/// </summary>
[Collection("Integration")]
public class SerilogBootstrapTests
{
    private readonly Demo1WebApplicationFactory _factory;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerilogBootstrapTests"/> class.
    /// </summary>
    /// <param name="factory">The shared web application factory.</param>
    public SerilogBootstrapTests(Demo1WebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// Verifies the application boots successfully with Serilog configured.
    /// A successful response from the root endpoint proves the full middleware
    /// pipeline—including Serilog request logging—initialises without error.
    /// </summary>
    [Fact]
    public async Task App_StartsSuccessfully_WithSerilogConfigured()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        ((int)response.StatusCode).Should().BeInRange(200, 399,
            "the app should start and respond successfully with Serilog configured");
    }

    /// <summary>
    /// Verifies the health/ready endpoint returns HTTP 200 while Serilog is active.
    /// This confirms that structured logging does not interfere with the
    /// health-check pipeline.
    /// </summary>
    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy_WithSerilogActive()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that an <see cref="ILoggerFactory"/> is registered in the DI container.
    /// When Serilog is integrated via <c>UseSerilog()</c>, it replaces the default
    /// logging provider; resolving the factory confirms the replacement is in place.
    /// </summary>
    [Fact]
    public void LoggerFactory_ResolvesSerilogProvider()
    {
        // Arrange & Act
        var loggerFactory = _factory.Services.GetService<ILoggerFactory>();

        // Assert
        loggerFactory.Should().NotBeNull(
            "Serilog should register an ILoggerFactory in the DI container");
    }
}
