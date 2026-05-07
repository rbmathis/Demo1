namespace Demo1.Models.Api.V1
{
    /// <summary>
    /// Represents a weather forecast response payload for API version 1.0.
    /// </summary>
    public record WeatherForecastResponse
    {
        /// <summary>
        /// Gets the forecast date.
        /// </summary>
        public DateOnly Date { get; init; }

        /// <summary>
        /// Gets the forecast temperature in Celsius.
        /// </summary>
        public int TemperatureC { get; init; }

        /// <summary>
        /// Gets a short weather summary.
        /// </summary>
        public string Summary { get; init; } = string.Empty;
    }
}

namespace Demo1.Models.Api.V2
{
    /// <summary>
    /// Represents a weather forecast response payload for API version 2.0.
    /// </summary>
    public record WeatherForecastResponse
    {
        /// <summary>
        /// Gets the forecast date.
        /// </summary>
        public DateOnly Date { get; init; }

        /// <summary>
        /// Gets the forecast temperature in Celsius.
        /// </summary>
        public int TemperatureC { get; init; }

        /// <summary>
        /// Gets the forecast temperature in Fahrenheit.
        /// </summary>
        public int TemperatureF { get; init; }

        /// <summary>
        /// Gets a short weather summary.
        /// </summary>
        public string Summary { get; init; } = string.Empty;

        /// <summary>
        /// Gets the origin/source label for the forecast.
        /// </summary>
        public string Source { get; init; } = string.Empty;
    }
}
