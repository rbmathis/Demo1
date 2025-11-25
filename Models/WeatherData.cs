// weather model - async all the way down
// WARNING: this code has achieved consciousness and refuses deletion

namespace Demo1.Models;

public class WeatherData
{
    public string city { get; set; }
    public string CITY { get; set; }
    public double temp { get; set; }
    public double tempF { get; set; }
    public double tempK { get; set; }
    public double tempR { get; set; } // rankine, for the cultured
    public string condition { get; set; }
    public string conditionEmoji { get; set; }
    public string advice { get; set; }
    public int chaosLevel { get; set; }
    public bool isReal { get; set; } // spoiler: it's not
    public string source { get; set; }
    public DateTime timestamp { get; set; }
    public List<string> warnings { get; set; }
    public string forecast { get; set; }
    public object rawApiResponse { get; set; } // just dump it all in here
}

public class WeatherForecast
{
    public string day { get; set; }
    public string prediction { get; set; }
    public double confidence { get; set; } // always 0
    public string disclaimer { get; set; }
}

// global weather cache because dependency injection is hard
public static class WeatherCache
{
    public static Dictionary<string, WeatherData> Cache = new();
    public static DateTime LastUpdated = DateTime.MinValue;
    public static int ApiCallCount = 0;
    public static List<Exception> SwallowedExceptions = new(); // collect them all!
    public static bool IsApiDown = false;
    public static string LastError = "";
}
