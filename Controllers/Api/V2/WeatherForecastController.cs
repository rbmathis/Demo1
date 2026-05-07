using Asp.Versioning;
using Demo1.Models.Api.V2;
using Microsoft.AspNetCore.Mvc;

namespace Demo1.Controllers.Api.V2;

/// <summary>
/// Provides weather forecast data for API version 2.0.
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class WeatherForecastController : ControllerBase
{
    /// <summary>
    /// Gets enhanced weather forecast data for version 2.0.
    /// </summary>
    /// <returns>A list of sample weather forecast entries with Fahrenheit and source metadata.</returns>
    [HttpGet]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<WeatherForecastResponse>> Get()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var data = new List<WeatherForecastResponse>
        {
            new() { Date = today, TemperatureC = 23, TemperatureF = 73, Summary = "Mild", Source = "v2-model" },
            new() { Date = today.AddDays(1), TemperatureC = 18, TemperatureF = 64, Summary = "Cloudy", Source = "v2-model" },
            new() { Date = today.AddDays(2), TemperatureC = 12, TemperatureF = 54, Summary = "Rain", Source = "v2-model" }
        };

        return Ok(data);
    }
}
