using System.Collections.Concurrent;
using Demo1.Models;

namespace Demo1.Services;

/// <summary>
/// Singleton implementation of <see cref="IPerformanceMetricsService"/> that stores
/// time-series performance data in memory with a configurable cap.
/// </summary>
public class PerformanceMetricsService : IPerformanceMetricsService
{
    private readonly ConcurrentQueue<PerformanceMetric> _metrics = new();
    private readonly IConfiguration _configuration;
    private const int MaxEntries = 1000;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceMetricsService"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration for reading budget thresholds.</param>
    public PerformanceMetricsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public void ReportMetric(PerformanceMetric metric)
    {
        _metrics.Enqueue(metric);

        // Enforce cap by dequeuing oldest entries
        while (_metrics.Count > MaxEntries)
        {
            _metrics.TryDequeue(out _);
        }
    }

    /// <inheritdoc />
    public IEnumerable<PerformanceMetric> GetHistory(string metricName, int minutes = 60)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-minutes);
        return _metrics
            .Where(m => m.MetricName.Equals(metricName, StringComparison.OrdinalIgnoreCase)
                        && m.Timestamp >= cutoff)
            .OrderBy(m => m.Timestamp)
            .ToList();
    }

    /// <inheritdoc />
    public IEnumerable<PerformanceBudget> GetBudgets()
    {
        var budgets = new List<PerformanceBudget>();
        _configuration.GetSection("PerformanceBudgets").Bind(budgets);
        return budgets;
    }
}
