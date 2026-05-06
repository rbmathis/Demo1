using Demo1.Models;

namespace Demo1.Services;

/// <summary>
/// Provides performance metrics collection, storage, and budget retrieval.
/// </summary>
public interface IPerformanceMetricsService
{
    /// <summary>
    /// Reports a new performance metric measurement.
    /// </summary>
    /// <param name="metric">The performance metric to store.</param>
    void ReportMetric(PerformanceMetric metric);

    /// <summary>
    /// Gets the history of measurements for a specific metric within a time window.
    /// </summary>
    /// <param name="metricName">The metric name to filter by (e.g., "LCP", "CLS").</param>
    /// <param name="minutes">The number of minutes to look back (default: 60).</param>
    /// <returns>An enumerable of matching performance metrics.</returns>
    IEnumerable<PerformanceMetric> GetHistory(string metricName, int minutes = 60);

    /// <summary>
    /// Gets the configured performance budgets for all metrics.
    /// </summary>
    /// <returns>An enumerable of performance budget configurations.</returns>
    IEnumerable<PerformanceBudget> GetBudgets();
}
