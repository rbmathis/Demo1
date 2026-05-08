using Demo1.IntegrationTests.Fixtures;

namespace Demo1.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for the ComponentShowcase endpoints.
/// Verifies that the showcase index and preview pages render successfully through the full pipeline.
/// </summary>
[Collection("Integration")]
public class ComponentShowcaseControllerTests
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentShowcaseControllerTests"/> class.
    /// </summary>
    /// <param name="factory">The shared web application factory.</param>
    public ComponentShowcaseControllerTests(Demo1WebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// Verifies the component showcase index page returns a successful response.
    /// </summary>
    [Fact]
    public async Task Get_ComponentShowcase_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/ComponentShowcase");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the component showcase index page returns HTML content containing the page title.
    /// </summary>
    [Fact]
    public async Task Get_ComponentShowcase_ReturnsHtmlContent()
    {
        // Act
        var response = await _client.GetAsync("/ComponentShowcase");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Component Showcase");
    }

    /// <summary>
    /// Verifies that previewing a valid component name returns a successful response.
    /// </summary>
    [Fact]
    public async Task Get_ComponentShowcasePreview_ValidName_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/ComponentShowcase/Preview/ButtonShowcase");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that previewing a non-existent component name returns 404 Not Found.
    /// </summary>
    [Fact]
    public async Task Get_ComponentShowcasePreview_InvalidName_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/ComponentShowcase/Preview/NonExistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
