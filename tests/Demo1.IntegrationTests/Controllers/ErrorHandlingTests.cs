using Demo1.IntegrationTests.Fixtures;

namespace Demo1.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for error handling and 404 responses.
/// Verifies that invalid routes return appropriate error responses.
/// </summary>
[Collection("Integration")]
public class ErrorHandlingTests
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorHandlingTests"/> class.
    /// </summary>
    /// <param name="factory">The shared web application factory.</param>
    public ErrorHandlingTests(Demo1WebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });
    }

    /// <summary>
    /// Verifies that a request to a non-existent route returns 404.
    /// </summary>
    [Fact]
    public async Task Get_NonExistentRoute_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/this-route-does-not-exist-xyz");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that a request to a non-existent controller returns 404.
    /// </summary>
    [Fact]
    public async Task Get_NonExistentController_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/NonExistentController/Index");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
