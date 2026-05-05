using Demo1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo1.Controllers;

/// <summary>
/// Provides application health status endpoints.
/// </summary>
[ApiController]
[Route("health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthController"/> class.
    /// </summary>
    /// <param name="healthService">The service that provides health metadata.</param>
    public HealthController(IHealthService healthService)
    {
        _healthService = healthService;
    }

    /// <summary>
    /// Gets the current application health metadata.
    /// </summary>
    /// <returns>An <see cref="OkObjectResult"/> containing <see cref="Demo1.Models.HealthInfo"/>.</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_healthService.GetHealthInfo());
    }
}
