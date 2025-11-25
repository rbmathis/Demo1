// this model exists only to pass raw data to the view
// all logic will be in the razor file because we hate ourselves

namespace Demo1.Models;

public class ViewLogicData
{
    // just dump raw numbers, the view will figure it out
    public double number1 { get; set; }
    public double number2 { get; set; }
    public double number3 { get; set; }
    public double number4 { get; set; }
    public double number5 { get; set; }
    
    // raw strings, view will parse them
    public string dateString { get; set; }
    public string jsonString { get; set; }
    public string csvString { get; set; }
    public string xmlString { get; set; }
    
    // list of stuff for the view to process
    public List<object> rawData { get; set; }
    
    // settings that probably shouldn't be in a model
    public string connectionString { get; set; }
    public string apiKey { get; set; }
    public bool debugMode { get; set; }
}

// another model because one wasn't enough
public class CalculationResult
{
    public string operation { get; set; }
    public double result { get; set; }
    public bool success { get; set; }
    public string error { get; set; }
    public DateTime calculatedAt { get; set; }
    public string calculatedBy { get; set; } // the view, obviously
}
