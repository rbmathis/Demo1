using Demo1.Models;
using Demo1.Services;

namespace Demo1.UnitTests.Models;

/// <summary>
/// Validates model structure, default values, and property behavior for all view/data models.
/// </summary>
public class ModelValidationTests
{
    [Fact]
    public void SearchQuery_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var query = new SearchQuery();

        // Assert
        Assert.Equal(string.Empty, query.term);
        Assert.Equal("documents", query.table);
        Assert.Equal("id", query.orderBy);
        Assert.Equal(10, query.limit);
        Assert.False(query.unsafeMode);
        Assert.Equal(string.Empty, query.customWhere);
    }

    [Fact]
    public void SearchQuery_CanSetAllProperties()
    {
        // Arrange
        var query = new SearchQuery();

        // Act
        query.term = "test search";
        query.table = "users";
        query.orderBy = "name";
        query.limit = 50;
        query.unsafeMode = true;
        query.customWhere = "status = 'active'";

        // Assert
        Assert.Equal("test search", query.term);
        Assert.Equal("users", query.table);
        Assert.Equal("name", query.orderBy);
        Assert.Equal(50, query.limit);
        Assert.True(query.unsafeMode);
        Assert.Equal("status = 'active'", query.customWhere);
    }

    [Fact]
    public void WeatherData_CanSetAllProperties()
    {
        // Arrange
        var weather = new WeatherData();
        var timestamp = new DateTime(2026, 5, 7, 12, 0, 0, DateTimeKind.Utc);
        var warnings = new List<string> { "High winds", "UV alert" };
        var rawResponse = new { status = "ok" };

        // Act
        weather.city = "Austin";
        weather.CITY = "AUSTIN";
        weather.temp = 30.5;
        weather.tempF = 86.9;
        weather.tempK = 303.65;
        weather.tempR = 546.57;
        weather.condition = "Sunny";
        weather.conditionEmoji = "☀️";
        weather.advice = "Wear sunscreen";
        weather.chaosLevel = 7;
        weather.isReal = true;
        weather.source = "WeatherAPI";
        weather.timestamp = timestamp;
        weather.warnings = warnings;
        weather.forecast = "Clear skies ahead";
        weather.rawApiResponse = rawResponse;

        // Assert
        Assert.Equal("Austin", weather.city);
        Assert.Equal("AUSTIN", weather.CITY);
        Assert.Equal(30.5, weather.temp);
        Assert.Equal(86.9, weather.tempF);
        Assert.Equal(303.65, weather.tempK);
        Assert.Equal(546.57, weather.tempR);
        Assert.Equal("Sunny", weather.condition);
        Assert.Equal("☀️", weather.conditionEmoji);
        Assert.Equal("Wear sunscreen", weather.advice);
        Assert.Equal(7, weather.chaosLevel);
        Assert.True(weather.isReal);
        Assert.Equal("WeatherAPI", weather.source);
        Assert.Equal(timestamp, weather.timestamp);
        Assert.Same(warnings, weather.warnings);
        Assert.Equal("Clear skies ahead", weather.forecast);
        Assert.Same(rawResponse, weather.rawApiResponse);
    }

    [Fact]
    public void WeatherData_DefaultWarnings_IsEmptyList()
    {
        // Arrange & Act
        var weather = new WeatherData();

        // Assert
        Assert.NotNull(weather.warnings);
        Assert.Empty(weather.warnings);
    }

    [Fact]
    public void WeatherData_DefaultTimestamp_IsRecent()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var weather = new WeatherData();

        // Assert
        var after = DateTime.UtcNow;
        Assert.InRange(weather.timestamp, before.AddSeconds(-5), after.AddSeconds(5));
    }

    [Fact]
    public void ViewLogicData_DefaultNumbersAreZero()
    {
        // Arrange & Act
        var data = new ViewLogicData();

        // Assert
        Assert.Equal(0.0, data.number1);
        Assert.Equal(0.0, data.number2);
        Assert.Equal(0.0, data.number3);
        Assert.Equal(0.0, data.number4);
        Assert.Equal(0.0, data.number5);
    }

    [Fact]
    public void ViewLogicData_DefaultRawData_IsEmptyList()
    {
        // Arrange & Act
        var data = new ViewLogicData();

        // Assert
        Assert.NotNull(data.rawData);
        Assert.Empty(data.rawData);
    }

    [Fact]
    public void ViewLogicData_CanSetAllFields()
    {
        // Arrange
        var data = new ViewLogicData();
        var rawData = new List<object> { "item1", 42, true };

        // Act
        data.number1 = 1.1;
        data.number2 = 2.2;
        data.number3 = 3.3;
        data.number4 = 4.4;
        data.number5 = 5.5;
        data.dateString = "2026-05-07";
        data.jsonString = "{\"key\":\"value\"}";
        data.csvString = "a,b,c";
        data.xmlString = "<root/>";
        data.rawData = rawData;
        data.debugMode = true;

        // Assert
        Assert.Equal(1.1, data.number1);
        Assert.Equal(2.2, data.number2);
        Assert.Equal(3.3, data.number3);
        Assert.Equal(4.4, data.number4);
        Assert.Equal(5.5, data.number5);
        Assert.Equal("2026-05-07", data.dateString);
        Assert.Equal("{\"key\":\"value\"}", data.jsonString);
        Assert.Equal("a,b,c", data.csvString);
        Assert.Equal("<root/>", data.xmlString);
        Assert.Same(rawData, data.rawData);
        Assert.True(data.debugMode);
    }

    [Fact]
    public void InlineCssModel_DefaultItemsList_IsNull()
    {
        // Arrange & Act
        var model = new InlineCssModel();

        // Assert
        Assert.Null(model.items);
    }

    [Fact]
    public void InlineCssModel_CanAddItems()
    {
        // Arrange
        var model = new InlineCssModel();
        var items = new List<InlineCssItem>
        {
            new InlineCssItem
            {
                text = "Click me",
                style = "color: red;",
                onclick = "alert('hi')",
                isImportant = true
            }
        };

        // Act
        model.items = items;
        model.items.Add(new InlineCssItem
        {
            text = "Another item",
            style = "font-weight: bold;",
            onclick = "",
            isImportant = false
        });

        // Assert
        Assert.NotNull(model.items);
        Assert.Equal(2, model.items.Count);
        Assert.Equal("Click me", model.items[0].text);
        Assert.Equal("color: red;", model.items[0].style);
        Assert.True(model.items[0].isImportant);
        Assert.Equal("Another item", model.items[1].text);
        Assert.False(model.items[1].isImportant);
    }

    [Fact]
    public void ErrorViewModel_ShowRequestId_TrueWhenSet()
    {
        // Arrange & Act
        var model = new ErrorViewModel { RequestId = "abc" };

        // Assert
        Assert.True(model.ShowRequestId);
    }

    [Fact]
    public void ErrorViewModel_ShowRequestId_FalseWhenNull()
    {
        // Arrange & Act
        var model = new ErrorViewModel { RequestId = null };

        // Assert
        Assert.False(model.ShowRequestId);
    }

    [Fact]
    public void ErrorViewModel_ShowRequestId_FalseWhenEmpty()
    {
        // Arrange & Act
        var model = new ErrorViewModel { RequestId = "" };

        // Assert
        Assert.False(model.ShowRequestId);
    }

    [Fact]
    public void UserProfile_DefaultIsActive_IsTrue()
    {
        // Arrange & Act
        var profile = new UserProfile();

        // Assert
        Assert.True(profile.IsActive);
    }

    [Fact]
    public void UserProfile_DefaultName_IsDefaultUser()
    {
        // Arrange & Act
        var profile = new UserProfile();

        // Assert
        Assert.Equal("Default User", profile.Name);
    }

    [Fact]
    public void UserProfile_Id_IsNotEmpty()
    {
        // Arrange & Act
        var profile = new UserProfile();

        // Assert
        Assert.False(string.IsNullOrEmpty(profile.Id));
    }
}
