using System.ComponentModel.DataAnnotations;
using Demo1.Models;

namespace Demo1.UnitTests.Models;

public class WeatherDataValidationTests
{
    [Fact]
    public void WeatherData_WithTooLongCity_IsInvalid()
    {
        var model = new WeatherData
        {
            city = new string('X', WeatherData.MaxCityLength + 1),
            temp = 0,
        };

        var validationResults = Validate(model);

        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(WeatherData.city)));
    }

    [Fact]
    public void WeatherData_WithOutOfRangeTemperature_IsInvalid()
    {
        var model = new WeatherData
        {
            city = "Seattle",
            temp = WeatherData.MaxTemperatureCelsius + 1,
        };

        var validationResults = Validate(model);

        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(WeatherData.temp)));
    }

    [Fact]
    public void WeatherData_WithValidCityAndTemperature_IsValid()
    {
        var model = new WeatherData
        {
            city = "Seattle",
            temp = 72,
        };

        var validationResults = Validate(model);

        Assert.Empty(validationResults);
    }

    private static List<ValidationResult> Validate(WeatherData weatherData)
    {
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(weatherData, new ValidationContext(weatherData), validationResults, validateAllProperties: true);
        return validationResults;
    }
}
