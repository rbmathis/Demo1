using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demo1.Controllers;

/// <summary>
/// Controller for the Performance Budget Monitor dashboard and API endpoints.
/// </summary>
[Route("[controller]")]
public class PerformanceController : Controller
{
    private readonly IPerformanceMetricsService _metricsService;
    private readonly ILogger<PerformanceController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceController"/> class.
    /// </summary>
    /// <param name="metricsService">The performance metrics service.</param>
    /// <param name="logger">The logger instance.</param>
    public PerformanceController(IPerformanceMetricsService metricsService, ILogger<PerformanceController> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the Core Web Vitals performance dashboard with budget thresholds.
    /// </summary>
    /// <returns>The dashboard view populated with performance budgets.</returns>
    [HttpGet("Dashboard")]
    public IActionResult Dashboard()
    {
        var budgets = _metricsService.GetBudgets();
        return View(budgets);
    }

    /// <summary>
    /// API endpoint to receive performance metric reports from the client.
    /// </summary>
    /// <param name="metric">The performance metric measurement to record.</param>
    /// <returns>OK result on success, or BadRequest if the model is invalid.</returns>
    [HttpPost("Report")]
    public IActionResult Report([FromBody] PerformanceMetric metric)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _metricsService.ReportMetric(metric);
            _logger.LogDebug("Reported {MetricName} = {Value} for {PageUrl}",
                metric.MetricName, metric.Value, metric.PageUrl);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting performance metric");
            return StatusCode(500, "An error occurred while processing the metric.");
        }
    }

    /// <summary>
    /// API endpoint to retrieve the history of a specific metric over a time window.
    /// </summary>
    /// <param name="metricName">The metric name to query (e.g., "LCP", "CLS").</param>
    /// <param name="minutes">The number of minutes to look back (default: 60).</param>
    /// <returns>A JSON array of performance metric entries.</returns>
    [HttpGet("History")]
    public IActionResult History([FromQuery] string metricName, [FromQuery] int minutes = 60)
    {
        if (string.IsNullOrWhiteSpace(metricName))
        {
            return BadRequest("metricName is required.");
        }

        var history = _metricsService.GetHistory(metricName, minutes);
        return Json(history);
    }
}
