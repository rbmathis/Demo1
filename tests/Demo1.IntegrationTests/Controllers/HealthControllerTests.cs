using Demo1.IntegrationTests.Fixtures;

namespace Demo1.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for health check endpoints.
/// Verifies that health and readiness endpoints respond correctly.
/// </summary>
[Collection("Integration")]
public class HealthControllerTests
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthControllerTests"/> class.
    /// </summary>
    /// <param name="factory">The shared web application factory.</param>
    public HealthControllerTests(Demo1WebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// Verifies the health endpoint returns HTTP 200.
    /// </summary>
    [Fact]
    public async Task Get_HealthEndpoint_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the health endpoint returns JSON content.
    /// </summary>
    [Fact]
    public async Task Get_HealthEndpoint_ReturnsJsonContent()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        content.Should().Contain("version");
        content.Should().Contain("uptime");
        content.Should().Contain("timestamp");
        content.Should().Contain("environment");
    }

    /// <summary>
    /// Verifies the health/ready endpoint returns HTTP 200.
    /// </summary>
    [Fact]
    public async Task Get_HealthReadyEndpoint_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
