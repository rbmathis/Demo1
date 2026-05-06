using Demo1.IntegrationTests.Fixtures;

namespace Demo1.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for the HomeController endpoints.
/// Verifies that key pages render successfully through the full pipeline.
/// </summary>
[Collection("Integration")]
public class HomeControllerTests
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeControllerTests"/> class.
    /// </summary>
    /// <param name="factory">The shared web application factory.</param>
    public HomeControllerTests(Demo1WebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// Verifies the home page returns a successful response.
    /// </summary>
    [Fact]
    public async Task Get_HomePage_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the home page returns HTML content type.
    /// </summary>
    [Fact]
    public async Task Get_HomePage_ReturnsHtmlContent()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    /// <summary>
    /// Verifies the About Us page loads successfully.
    /// </summary>
    [Fact]
    public async Task Get_AboutUsPage_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/Home/AboutUs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the Privacy page loads successfully.
    /// </summary>
    [Fact]
    public async Task Get_PrivacyPage_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/Home/Privacy");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
