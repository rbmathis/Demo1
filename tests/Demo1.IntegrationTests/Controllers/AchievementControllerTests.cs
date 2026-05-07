using Demo1.IntegrationTests.Fixtures;

namespace Demo1.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for the AchievementController endpoints.
/// Verifies that achievement pages render successfully through the full pipeline.
/// </summary>
[Collection("Integration")]
public class AchievementControllerTests
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementControllerTests"/> class.
    /// </summary>
    /// <param name="factory">The shared web application factory.</param>
    public AchievementControllerTests(Demo1WebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// Verifies the trophy case page returns a successful response.
    /// </summary>
    [Fact]
    public async Task Get_TrophyCasePage_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/Achievement/TrophyCase");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the trophy case page contains expected badge names.
    /// </summary>
    [Fact]
    public async Task Get_TrophyCasePage_ContainsBadgeNames()
    {
        // Act
        var response = await _client.GetAsync("/Achievement/TrophyCase");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        content.Should().Contain("Explorer");
        content.Should().Contain("Speed Demon");
        content.Should().Contain("White Hat");
        content.Should().Contain("Benchmarker");
        content.Should().Contain("API Curious");
        content.Should().Contain("Completionist");
    }

    /// <summary>
    /// Verifies the synchronous anti-pattern page returns a successful response.
    /// </summary>
    [Fact]
    public async Task Get_SynchronousAntiPattern_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/Achievement/SynchronousAntiPattern");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the badges API endpoint returns JSON.
    /// </summary>
    [Fact]
    public async Task Get_BadgesApi_ReturnsJson()
    {
        // Act
        var response = await _client.GetAsync("/Achievement/api/badges");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}
