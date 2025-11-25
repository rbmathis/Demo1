// Data model for calculator view demonstration

namespace Demo1.Models;

/// <summary>
/// Data model for the view logic calculator demo.
/// </summary>
public class ViewLogicData
{
    public double number1 { get; set; }
    public double number2 { get; set; }
    public double number3 { get; set; }
    public double number4 { get; set; }
    public double number5 { get; set; }

    public string dateString { get; set; } = string.Empty;
    public string jsonString { get; set; } = string.Empty;
    public string csvString { get; set; } = string.Empty;
    public string xmlString { get; set; } = string.Empty;

    public List<object> rawData { get; set; } = new();

    public bool debugMode { get; set; }
}

/// <summary>
/// Result of a calculation operation.
/// </summary>
public class CalculationResult
{
    public string operation { get; set; } = string.Empty;
    public double result { get; set; }
    public bool success { get; set; }
    public string error { get; set; } = string.Empty;
    public DateTime calculatedAt { get; set; } = DateTime.UtcNow;
}
