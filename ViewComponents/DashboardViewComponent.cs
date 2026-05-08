using Demo1.Features;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace Demo1.ViewComponents;

/// <summary>
/// Renders dashboard summary cards for uptime and request performance.
/// </summary>
public class DashboardViewComponent : ViewComponent
{
    private readonly IUptimeService _uptimeService;
    private readonly IPerformanceMetricsService _performanceMetricsService;
    private readonly IFeatureManagerSnapshot _featureManager;
    private readonly ILogger<DashboardViewComponent> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewComponent"/> class.
    /// </summary>
    /// <param name="uptimeService">Service that provides application uptime.</param>
    /// <param name="performanceMetricsService">Service that provides performance metric history.</param>
    /// <param name="featureManager">Feature manager snapshot for rollout checks.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public DashboardViewComponent(
        IUptimeService uptimeService,
        IPerformanceMetricsService performanceMetricsService,
        IFeatureManagerSnapshot featureManager,
        ILogger<DashboardViewComponent> logger)
    {
        _uptimeService = uptimeService;
        _performanceMetricsService = performanceMetricsService;
        _featureManager = featureManager;
        _logger = logger;
    }

    /// <summary>
    /// Builds and returns dashboard cards for the home page.
    /// </summary>
    /// <returns>The dashboard view with card view models.</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.DashboardHomePage))
        {
            _logger.LogWarning("Dashboard component invoked while DashboardHomePage flag is disabled");
            return Content(string.Empty);
        }

        _logger.LogInformation("DashboardHomePage feature active, rendering dashboard");

        var uptime = _uptimeService.GetUptime();
        var requestDurationHistory = _performanceMetricsService.GetHistory("RequestDuration", 60)
            .Select(metric => metric.Value)
            .ToArray();
        var requestCountHistory = _performanceMetricsService.GetHistory("RequestCount", 60)
            .Select(metric => metric.Value)
            .ToArray();

        var cards = new[]
        {
            new DashboardCardViewModel
            {
                Title = "Uptime",
                Value = uptime > TimeSpan.Zero ? FormatUptime(uptime) : "N/A",
                Unit = string.Empty,
                CssClass = "dashboard-card-uptime",
                Icon = "bi-clock-history",
                SparklinePoints = Array.Empty<double>(),
            },
            BuildMetricCard(
                title: "Request Duration",
                unit: "ms",
                cssClass: "dashboard-card-duration",
                icon: "bi-speedometer2",
                values: requestDurationHistory),
            BuildMetricCard(
                title: "Request Count",
                unit: string.Empty,
                cssClass: "dashboard-card-count",
                icon: "bi-bar-chart",
                values: requestCountHistory),
            new DashboardCardViewModel
            {
                Title = "Health Status",
                Value = uptime > TimeSpan.Zero ? "Healthy" : "N/A",
                Unit = string.Empty,
                CssClass = "dashboard-card-health",
                Icon = "bi-heart-pulse",
                SparklinePoints = Array.Empty<double>(),
            },
        };

        return View(cards);
    }

    private static DashboardCardViewModel BuildMetricCard(
        string title,
        string unit,
        string cssClass,
        string icon,
        IReadOnlyList<double> values)
    {
        if (values.Count == 0)
        {
            return new DashboardCardViewModel
            {
                Title = title,
                Value = "N/A",
                Unit = unit,
                CssClass = cssClass,
                Icon = icon,
                SparklinePoints = Array.Empty<double>(),
            };
        }

        return new DashboardCardViewModel
        {
            Title = title,
            Value = values[^1].ToString("0.##"),
            Unit = unit,
            CssClass = cssClass,
            Icon = icon,
            SparklinePoints = NormalizeSparkline(values),
        };
    }

    private static IReadOnlyList<double> NormalizeSparkline(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
        {
            return Array.Empty<double>();
        }

        var min = values.Min();
        var max = values.Max();
        if (Math.Abs(max - min) < double.Epsilon)
        {
            return values.Select(_ => 0.5d).ToArray();
        }

        var range = max - min;
        return values.Select(value => (value - min) / range).ToArray();
    }

    private static string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
        {
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h";
        }

        if (uptime.TotalHours >= 1)
        {
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
        }

        return $"{Math.Max(0, uptime.Minutes)}m";
    }
}
