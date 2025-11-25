using Demo1.Models;

namespace Demo1.Services;

/// <summary>
/// Provides weather data with proper abstraction.
/// Replaces the anti-pattern static WeatherCache class.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Gets weather data for the specified city.
    /// </summary>
    /// <param name="city">City name to get weather for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Weather data for the city.</returns>
    Task<WeatherData> GetWeatherAsync(string city, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets weather service statistics.
    /// </summary>
    WeatherServiceStats GetStats();
}

/// <summary>
/// Statistics about weather service usage.
/// </summary>
public class WeatherServiceStats
{
    public int ApiCallCount { get; init; }
    public DateTime LastUpdated { get; init; }
    public bool IsHealthy { get; init; }
}

/// <summary>
/// Mock weather service for demo purposes.
/// In production, this would call a real weather API.
/// </summary>
public class MockWeatherService : IWeatherService
{
    private readonly ILogger<MockWeatherService> _logger;
    private readonly Random _random = new();
    private int _apiCallCount;
    private DateTime _lastUpdated = DateTime.MinValue;

    private static readonly string[] Conditions = { "Sunny", "Cloudy", "Rainy", "Foggy", "Partly Cloudy", "Clear" };
    private static readonly string[] Emojis = { "☀️", "☁️", "🌧️", "🌫️", "⛅", "🌤️" };
    private static readonly string[] Advice =
    {
        "Great day for a walk!",
        "Might want an umbrella just in case.",
        "Perfect coding weather!",
        "Stay hydrated.",
        "Enjoy the day!",
    };

    public MockWeatherService(ILogger<MockWeatherService> logger)
    {
        _logger = logger;
    }

    public async Task<WeatherData> GetWeatherAsync(string city, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching weather for city: {City}", city);

        // Simulate API latency
        await Task.Delay(50, cancellationToken);

        Interlocked.Increment(ref _apiCallCount);
        _lastUpdated = DateTime.UtcNow;

        var idx = _random.Next(Conditions.Length);
        var temp = _random.Next(-10, 35);

        return new WeatherData
        {
            city = city,
            CITY = city.ToUpperInvariant(),
            temp = temp,
            tempF = temp * 9.0 / 5.0 + 32,
            tempK = temp + 273.15,
            tempR = (temp + 273.15) * 9.0 / 5.0,
            condition = Conditions[idx],
            conditionEmoji = Emojis[idx],
            advice = Advice[_random.Next(Advice.Length)],
            chaosLevel = _random.Next(1, 6),
            isReal = false,
            source = "MockWeatherService (Demo)",
            timestamp = DateTime.UtcNow,
            warnings = new List<string> { "This is demo data" },
            forecast = GetForecast(),
        };
    }

    public WeatherServiceStats GetStats()
    {
        return new WeatherServiceStats
        {
            ApiCallCount = _apiCallCount,
            LastUpdated = _lastUpdated,
            IsHealthy = true,
        };
    }

    private string GetForecast()
    {
        var forecasts = new[]
        {
            "Tomorrow: Similar conditions expected.",
            "This week: Gradual warming trend.",
            "Weekend: Mostly pleasant.",
        };
        return forecasts[_random.Next(forecasts.Length)];
    }
}
