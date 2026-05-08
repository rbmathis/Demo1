using Demo1.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="MockWeatherService"/> verifying weather data generation, temperature conversions, and service stats.
/// </summary>
public class MockWeatherServiceTests
{
    private readonly MockWeatherService _service;

    public MockWeatherServiceTests()
    {
        var logger = Mock.Of<ILogger<MockWeatherService>>();
        _service = new MockWeatherService(logger);
    }

    /// <summary>
    /// Verifies that the returned weather data contains the requested city name.
    /// </summary>
    [Fact]
    public async Task GetWeatherAsync_ReturnsCityName()
    {
        // Arrange
        var city = "Seattle";

        // Act
        var result = await _service.GetWeatherAsync(city);

        // Assert
        Assert.Equal("Seattle", result.city);
    }

    /// <summary>
    /// Verifies that the CITY field is the uppercase invariant version of the city name.
    /// </summary>
    [Fact]
    public async Task GetWeatherAsync_ReturnsCityUppercase()
    {
        // Arrange
        var city = "Seattle";

        // Act
        var result = await _service.GetWeatherAsync(city);

        // Assert
        Assert.Equal("SEATTLE", result.CITY);
    }

    /// <summary>
    /// Verifies that the temperature falls within the expected range of -10 to 34 inclusive.
    /// </summary>
    [Fact]
    public async Task GetWeatherAsync_TemperatureInValidRange()
    {
        // Arrange
        var city = "TestCity";

        // Act
        var result = await _service.GetWeatherAsync(city);

        // Assert
        Assert.InRange(result.temp, -10, 34);
    }

    /// <summary>
    /// Verifies that Fahrenheit conversion is correct: F = C * 9/5 + 32.
    /// </summary>
    [Fact]
    public async Task GetWeatherAsync_TempF_IsCorrectConversion()
    {
        // Arrange
        var city = "TestCity";

        // Act
        var result = await _service.GetWeatherAsync(city);

        // Assert
        var expectedF = result.temp * 9.0 / 5.0 + 32;
        Assert.Equal(expectedF, result.tempF, precision: 5);
    }

    /// <summary>
    /// Verifies that Kelvin conversion is correct: K = C + 273.15.
    /// </summary>
    [Fact]
    public async Task GetWeatherAsync_TempK_IsCorrectConversion()
    {
        // Arrange
        var city = "TestCity";

        // Act
        var result = await _service.GetWeatherAsync(city);

        // Assert
        var expectedK = result.temp + 273.15;
        Assert.Equal(expectedK, result.tempK, precision: 5);
    }

    /// <summary>
    /// Verifies that the weather data is flagged as not real (mock data).
    /// </summary>
    [Fact]
    public async Task GetWeatherAsync_IsNotReal()
    {
        // Arrange
        var city = "TestCity";

        // Act
        var result = await _service.GetWeatherAsync(city);

        // Assert
        Assert.False(result.isReal);
    }

    /// <summary>
    /// Verifies that the source is correctly attributed to the mock service.
    /// </summary>
    [Fact]
    public async Task GetWeatherAsync_HasValidSource()
    {
        // Arrange
        var city = "TestCity";

        // Act
        var result = await _service.GetWeatherAsync(city);

        // Assert
        Assert.Equal("MockWeatherService (Demo)", result.source);
    }

    /// <summary>
    /// Verifies that the warnings list is populated with at least one entry.
    /// </summary>
    [Fact]
    public async Task GetWeatherAsync_HasWarnings()
    {
        // Arrange
        var city = "TestCity";

        // Act
        var result = await _service.GetWeatherAsync(city);

        // Assert
        Assert.NotNull(result.warnings);
        Assert.NotEmpty(result.warnings);
    }

    /// <summary>
    /// Verifies that the forecast field is populated with a non-empty string.
    /// </summary>
    [Fact]
    public async Task GetWeatherAsync_HasForecast()
    {
        // Arrange
        var city = "TestCity";

        // Act
        var result = await _service.GetWeatherAsync(city);

        // Assert
        Assert.NotNull(result.forecast);
        Assert.NotEmpty(result.forecast);
    }

    /// <summary>
    /// Verifies that the ApiCallCount increments after a weather request.
    /// </summary>
    [Fact]
    public async Task GetWeatherAsync_IncrementsApiCallCount()
    {
        // Arrange
        var statsBefore = _service.GetStats();

        // Act
        await _service.GetWeatherAsync("TestCity");
        var statsAfter = _service.GetStats();

        // Assert
        Assert.Equal(statsBefore.ApiCallCount + 1, statsAfter.ApiCallCount);
    }

    /// <summary>
    /// Verifies that the service reports healthy status.
    /// </summary>
    [Fact]
    public void GetStats_IsHealthy()
    {
        // Arrange & Act
        var stats = _service.GetStats();

        // Assert
        Assert.True(stats.IsHealthy);
    }

    /// <summary>
    /// Verifies that a pre-cancelled cancellation token causes a TaskCanceledException.
    /// </summary>
    [Fact]
    public async Task GetWeatherAsync_RespectsCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _service.GetWeatherAsync("TestCity", cts.Token));
    }
}
