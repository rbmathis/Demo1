using Demo1.IntegrationTests.Fixtures;

namespace Demo1.IntegrationTests.Controllers;

/// <summary>
/// Integration tests verifying that all anti-pattern demonstration routes
/// are accessible and return successful HTTP responses.
/// </summary>
[Collection("Integration")]
public class HomeAntiPatternRouteTests
{
    private readonly HttpClient _client;

    public HomeAntiPatternRouteTests(Demo1WebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// Verifies the RawSqlSearch page loads successfully without query parameters.
    /// </summary>
    [Fact]
    public async Task Get_RawSqlSearch_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Home/RawSqlSearch");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the RawSqlSearch page loads successfully with a search query parameter.
    /// </summary>
    [Fact]
    public async Task Get_RawSqlSearchWithQuery_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Home/RawSqlSearch?q=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the CallbackHellWeather page loads successfully without parameters.
    /// </summary>
    [Fact]
    public async Task Get_CallbackHellWeather_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Home/CallbackHellWeather");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the CallbackHellWeather page loads successfully with a city parameter.
    /// </summary>
    [Fact]
    public async Task Get_CallbackHellWeatherWithCity_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Home/CallbackHellWeather?city=London");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the InlineCssHell page loads successfully without parameters.
    /// </summary>
    [Fact]
    public async Task Get_InlineCssHell_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Home/InlineCssHell");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the InlineCssHell page loads successfully with a high chaos parameter.
    /// </summary>
    [Fact]
    public async Task Get_InlineCssHellWithChaos_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Home/InlineCssHell?chaos=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the ViewLogicCalculator page loads successfully.
    /// </summary>
    [Fact]
    public async Task Get_ViewLogicCalculator_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Home/ViewLogicCalculator");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the GodObjectProfile page loads successfully via GET.
    /// </summary>
    [Fact]
    public async Task Get_GodObjectProfile_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Home/GodObjectProfile");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies the RawSqlSearch page returns HTML content type.
    /// </summary>
    [Fact]
    public async Task Get_RawSqlSearch_ReturnsHtmlContentType()
    {
        var response = await _client.GetAsync("/Home/RawSqlSearch");

        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }
}
