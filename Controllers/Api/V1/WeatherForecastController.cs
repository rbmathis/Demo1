using Asp.Versioning;
using Demo1.Models.Api.V1;
using Microsoft.AspNetCore.Mvc;

namespace Demo1.Controllers.Api.V1;

/// <summary>
/// Provides weather forecast data for API version 1.0.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public class WeatherForecastController : ControllerBase
{
    /// <summary>
    /// Gets weather forecast data for version 1.0.
    /// </summary>
    /// <returns>A list of sample weather forecast entries.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<WeatherForecastResponse>> Get()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var data = new List<WeatherForecastResponse>
        {
            new() { Date = today, TemperatureC = 23, Summary = "Mild" },
            new() { Date = today.AddDays(1), TemperatureC = 18, Summary = "Cloudy" },
            new() { Date = today.AddDays(2), TemperatureC = 12, Summary = "Rain" }
        };

        return Ok(data);
    }
}
