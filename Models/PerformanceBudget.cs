namespace Demo1.Models;

/// <summary>
/// Represents a performance budget threshold for a Core Web Vitals metric.
/// </summary>
public class PerformanceBudget
{
    /// <summary>
    /// Gets or sets the metric name (e.g., "LCP", "CLS", "TTFB", "FID").
    /// </summary>
    public string MetricName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the warning threshold value. Values above this indicate degraded performance.
    /// </summary>
    public double WarningThreshold { get; set; }

    /// <summary>
    /// Gets or sets the error threshold value. Values above this indicate poor performance.
    /// </summary>
    public double ErrorThreshold { get; set; }

    /// <summary>
    /// Gets or sets the unit of measurement ("ms" or "" for unitless metrics).
    /// </summary>
    public string Unit { get; set; } = "ms";
}
