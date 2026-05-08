using System.ComponentModel.DataAnnotations;

namespace Demo1.Models;

/// <summary>
/// View model representing a single dashboard card and optional sparkline data.
/// </summary>
public class DashboardCardViewModel
{
    /// <summary>
    /// Gets or sets the dashboard card title.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display value for the card.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value unit text.
    /// </summary>
    [StringLength(20)]
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CSS class used to style the card.
    /// </summary>
    [StringLength(100)]
    public string CssClass { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon name or CSS token for the card.
    /// </summary>
    [StringLength(100)]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalized sparkline points in the range 0..1.
    /// </summary>
    public IReadOnlyList<double> SparklinePoints { get; set; } = Array.Empty<double>();
}
