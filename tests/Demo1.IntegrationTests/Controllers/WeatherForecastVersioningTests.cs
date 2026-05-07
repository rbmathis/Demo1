using System.Text.Json.Nodes;
using Demo1.IntegrationTests.Fixtures;

namespace Demo1.IntegrationTests.Controllers;

[Collection("Integration")]
public class WeatherForecastVersioningTests
{
    private readonly HttpClient _client;

    public WeatherForecastVersioningTests(Demo1WebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Get_V1Endpoint_ReturnsV1Shape()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/weatherforecast");
        var payload = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var first = GetFirstItem(payload);
        first.Should().ContainKey("date");
        first.Should().ContainKey("temperatureC");
        first.Should().ContainKey("summary");
        first.Should().NotContainKey("temperatureF");
        first.Should().NotContainKey("source");
    }

    [Fact]
    public async Task Get_V2Endpoint_ReturnsEnhancedShape()
    {
        // Act
        var response = await _client.GetAsync("/api/v2/weatherforecast");
        var payload = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var first = GetFirstItem(payload);
        first.Should().ContainKey("date");
        first.Should().ContainKey("temperatureC");
        first.Should().ContainKey("summary");
        first.Should().ContainKey("temperatureF");
        first.Should().ContainKey("source");
    }

    [Theory]
    [InlineData("/api/v1/weatherforecast")]
    [InlineData("/api/v2/weatherforecast")]
    public async Task Get_VersionedRoutes_ResolveSuccessfully(string route)
    {
        // Act
        var response = await _client.GetAsync(route);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_WithoutVersion_ResolvesToV1ByDefault()
    {
        // Act
        var response = await _client.GetAsync("/api/weatherforecast");
        var payload = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var first = GetFirstItem(payload);
        first.Should().ContainKey("date");
        first.Should().ContainKey("temperatureC");
        first.Should().ContainKey("summary");
        first.Should().NotContainKey("temperatureF");
        first.Should().NotContainKey("source");
    }

    private static JsonObject GetFirstItem(string payload)
    {
        var json = JsonNode.Parse(payload)!.AsArray();
        json.Should().NotBeEmpty();

        return json[0]!.AsObject();
    }
}
