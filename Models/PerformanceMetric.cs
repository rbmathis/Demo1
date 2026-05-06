using System.ComponentModel.DataAnnotations;

namespace Demo1.Models;

/// <summary>
/// Represents a single Core Web Vitals performance metric measurement.
/// </summary>
public class PerformanceMetric
{
    /// <summary>
    /// Gets or sets the unique identifier for this metric entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the metric name (e.g., "LCP", "CLS", "TTFB", "FID").
    /// </summary>
    [Required]
    [StringLength(10)]
    public string MetricName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the measured value of the metric.
    /// </summary>
    [Range(0, double.MaxValue)]
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets the unit of measurement ("ms" or "" for unitless metrics like CLS).
    /// </summary>
    [StringLength(5)]
    public string Unit { get; set; } = "ms";

    /// <summary>
    /// Gets or sets the timestamp when the metric was recorded.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the page URL where the metric was collected.
    /// </summary>
    [StringLength(2048)]
    public string PageUrl { get; set; } = string.Empty;
}
