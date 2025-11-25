// Weather data model for demonstration purposes

namespace Demo1.Models;

/// <summary>
/// Represents weather data for a location.
/// </summary>
public class WeatherData
{
    public string city { get; set; } = string.Empty;
    public string CITY { get; set; } = string.Empty;
    public double temp { get; set; }
    public double tempF { get; set; }
    public double tempK { get; set; }
    public double tempR { get; set; }
    public string condition { get; set; } = string.Empty;
    public string conditionEmoji { get; set; } = string.Empty;
    public string advice { get; set; } = string.Empty;
    public int chaosLevel { get; set; }
    public bool isReal { get; set; }
    public string source { get; set; } = string.Empty;
    public DateTime timestamp { get; set; } = DateTime.UtcNow;
    public List<string> warnings { get; set; } = new();
    public string forecast { get; set; } = string.Empty;
    public object? rawApiResponse { get; set; }
}

/// <summary>
/// Represents a weather forecast entry.
/// </summary>
public class WeatherForecast
{
    public string day { get; set; } = string.Empty;
    public string prediction { get; set; } = string.Empty;
    public double confidence { get; set; }
    public string disclaimer { get; set; } = string.Empty;
}
