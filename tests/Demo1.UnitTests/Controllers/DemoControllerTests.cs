using Demo1.Controllers;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Controllers;

/// <summary>
/// Tests for the anti-pattern demo actions in DemoController.
/// These actions demonstrate refactored "bad code" patterns while retaining chaotic UI styling.
/// </summary>
public class DemoControllerTests
{
    private static (DemoController controller, Mock<ISearchService> searchMock, Mock<IWeatherService> weatherMock, Mock<IStyleGeneratorService> styleMock) CreateControllerWithMocks()
    {
        var loggerMock = new Mock<ILogger<DemoController>>();
        var searchMock = new Mock<ISearchService>();
        var weatherMock = new Mock<IWeatherService>();
        var styleMock = new Mock<IStyleGeneratorService>();

        searchMock.Setup(s => s.SearchAsync(It.IsAny<SearchQuery>()))
            .ReturnsAsync(new List<SearchResult>());
        searchMock.Setup(s => s.GetRecentQueries(It.IsAny<int>()))
            .Returns(new List<string>().AsReadOnly());
        searchMock.Setup(s => s.TotalQueryCount).Returns(0);

        weatherMock.Setup(w => w.GetWeatherAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeatherData { city = "Test", temp = 20, condition = "Sunny", isReal = false });
        weatherMock.Setup(w => w.GetStats())
            .Returns(new WeatherServiceStats { ApiCallCount = 5, IsHealthy = true, LastUpdated = DateTime.UtcNow });

        styleMock.Setup(s => s.GetRandomFont()).Returns("Comic Sans MS");
        styleMock.Setup(s => s.GetRandomColor()).Returns("#FF00FF");
        styleMock.Setup(s => s.GenerateChaosStyle()).Returns("color: #FF00FF; font-size: 24px;");

        var controller = new DemoController(
            loggerMock.Object,
            searchMock.Object,
            weatherMock.Object,
            styleMock.Object
        );

        return (controller, searchMock, weatherMock, styleMock);
    }

    // ===================================================================
    // RawSqlSearch Tests
    // ===================================================================

    [Fact]
    public async Task RawSqlSearch_ReturnsViewResult()
    {
        // Arrange
        var (controller, _, _, _) = CreateControllerWithMocks();

        // Act
        var result = await controller.RawSqlSearch();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task RawSqlSearch_CallsSearchService()
    {
        // Arrange
        var (controller, searchMock, _, _) = CreateControllerWithMocks();

        // Act
        await controller.RawSqlSearch("hello");

        // Assert
        searchMock.Verify(s => s.SearchAsync(It.IsAny<SearchQuery>()), Times.Once);
    }

    [Fact]
    public async Task RawSqlSearch_SetsViewBagResults()
    {
        // Arrange
        var expectedResults = new List<SearchResult>
        {
            new() { id = 1, title = "Result 1", description = "Desc", category = "docs" }
        };
        var (controller, searchMock, _, _) = CreateControllerWithMocks();
        searchMock.Setup(s => s.SearchAsync(It.IsAny<SearchQuery>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await controller.RawSqlSearch("test");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(expectedResults, viewResult.ViewData["Results"]);
    }

    [Fact]
    public async Task RawSqlSearch_SetsViewBagQuery()
    {
        // Arrange
        var (controller, _, _, _) = CreateControllerWithMocks();

        // Act
        var result = await controller.RawSqlSearch("findme");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var query = Assert.IsType<SearchQuery>(viewResult.ViewData["Query"]);
        Assert.Equal("findme", query.term);
    }

    [Fact]
    public async Task RawSqlSearch_DefaultParams_UsesDefaults()
    {
        // Arrange
        var (controller, searchMock, _, _) = CreateControllerWithMocks();
        SearchQuery? capturedQuery = null;
        searchMock.Setup(s => s.SearchAsync(It.IsAny<SearchQuery>()))
            .Callback<SearchQuery>(q => capturedQuery = q)
            .ReturnsAsync(new List<SearchResult>());

        // Act
        await controller.RawSqlSearch();

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal("", capturedQuery.term);
        Assert.Equal("users", capturedQuery.table);
        Assert.Equal("id", capturedQuery.orderBy);
        Assert.Equal("", capturedQuery.customWhere);
    }

    [Fact]
    public async Task RawSqlSearch_WithQuery_PassesTermToService()
    {
        // Arrange
        var (controller, searchMock, _, _) = CreateControllerWithMocks();
        SearchQuery? capturedQuery = null;
        searchMock.Setup(s => s.SearchAsync(It.IsAny<SearchQuery>()))
            .Callback<SearchQuery>(q => capturedQuery = q)
            .ReturnsAsync(new List<SearchResult>());

        // Act
        await controller.RawSqlSearch("test");

        // Assert
        Assert.NotNull(capturedQuery);
        Assert.Equal("test", capturedQuery.term);
    }

    // ===================================================================
    // CallbackHellWeather Tests
    // ===================================================================

    [Fact]
    public async Task CallbackHellWeather_ReturnsViewResult()
    {
        // Arrange
        var (controller, _, _, _) = CreateControllerWithMocks();

        // Act
        var result = await controller.CallbackHellWeather();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task CallbackHellWeather_SetsViewBagWeather()
    {
        // Arrange
        var (controller, _, weatherMock, _) = CreateControllerWithMocks();
        var expectedWeather = new WeatherData { city = "Chaosville", temp = 22, condition = "Chaotic", isReal = false };
        weatherMock.Setup(w => w.GetWeatherAsync("Chaosville", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedWeather);

        // Act
        var result = await controller.CallbackHellWeather();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(expectedWeather, viewResult.ViewData["Weather"]);
    }

    [Fact]
    public async Task CallbackHellWeather_DefaultCity_IsChaosville()
    {
        // Arrange
        var (controller, _, weatherMock, _) = CreateControllerWithMocks();

        // Act
        await controller.CallbackHellWeather();

        // Assert
        weatherMock.Verify(w => w.GetWeatherAsync("Chaosville", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CallbackHellWeather_CustomCity_PassedToService()
    {
        // Arrange
        var (controller, _, weatherMock, _) = CreateControllerWithMocks();

        // Act
        await controller.CallbackHellWeather("Paris");

        // Assert
        weatherMock.Verify(w => w.GetWeatherAsync("Paris", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CallbackHellWeather_WhenServiceThrows_SetsErrors()
    {
        // Arrange
        var (controller, _, weatherMock, _) = CreateControllerWithMocks();
        weatherMock.Setup(w => w.GetWeatherAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service exploded"));

        // Act
        var result = await controller.CallbackHellWeather("Boom City");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var errors = Assert.IsType<List<string>>(viewResult.ViewData["Errors"]);
        Assert.Single(errors);
        Assert.Contains("Service exploded", errors[0]);
    }

    [Fact]
    public async Task CallbackHellWeather_WhenServiceThrows_StillReturnsView()
    {
        // Arrange
        var (controller, _, weatherMock, _) = CreateControllerWithMocks();
        weatherMock.Setup(w => w.GetWeatherAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Total meltdown"));

        // Act
        var result = await controller.CallbackHellWeather();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task CallbackHellWeather_SetsApiCallsFromStats()
    {
        // Arrange
        var (controller, _, weatherMock, _) = CreateControllerWithMocks();
        weatherMock.Setup(w => w.GetStats())
            .Returns(new WeatherServiceStats { ApiCallCount = 42, IsHealthy = true, LastUpdated = DateTime.UtcNow });

        // Act
        var result = await controller.CallbackHellWeather();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(42, viewResult.ViewData["ApiCalls"]);
    }

    [Fact]
    public async Task CallbackHellWeather_HealthyService_ShowsHealthy()
    {
        // Arrange
        var (controller, _, weatherMock, _) = CreateControllerWithMocks();
        weatherMock.Setup(w => w.GetStats())
            .Returns(new WeatherServiceStats { ApiCallCount = 1, IsHealthy = true, LastUpdated = DateTime.UtcNow });

        // Act
        var result = await controller.CallbackHellWeather();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var cacheStatus = Assert.IsType<string>(viewResult.ViewData["CacheStatus"]);
        Assert.Contains("Healthy", cacheStatus);
    }

    // ===================================================================
    // InlineCssHell Tests
    // ===================================================================

    [Fact]
    public void InlineCssHell_ReturnsViewResult()
    {
        // Arrange
        var (controller, _, _, _) = CreateControllerWithMocks();

        // Act
        var result = controller.InlineCssHell();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void InlineCssHell_ChaosAbove11_ClampedTo11()
    {
        // Arrange
        var (controller, _, _, _) = CreateControllerWithMocks();

        // Act
        var result = controller.InlineCssHell(99);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(11, viewResult.ViewData["ChaosLevel"]);
    }

    [Fact]
    public void InlineCssHell_ChaosBelow1_ClampedTo1()
    {
        // Arrange
        var (controller, _, _, _) = CreateControllerWithMocks();

        // Act
        var result = controller.InlineCssHell(-5);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(1, viewResult.ViewData["ChaosLevel"]);
    }

    [Fact]
    public void InlineCssHell_SetsModelWithItems()
    {
        // Arrange
        var (controller, _, _, _) = CreateControllerWithMocks();

        // Act
        var result = controller.InlineCssHell();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<InlineCssModel>(viewResult.ViewData["Model"]);
        Assert.Equal(7, model.items.Count);
    }

    [Fact]
    public void InlineCssHell_CallsStyleGenerator()
    {
        // Arrange
        var (controller, _, _, styleMock) = CreateControllerWithMocks();

        // Act
        controller.InlineCssHell();

        // Assert
        styleMock.Verify(s => s.GetRandomFont(), Times.AtLeastOnce);
        styleMock.Verify(s => s.GetRandomColor(), Times.AtLeastOnce);
        styleMock.Verify(s => s.GenerateChaosStyle(), Times.AtLeastOnce);
    }

    [Fact]
    public void InlineCssHell_ChaosAbove5_EnablesChaosFlag()
    {
        // Arrange
        var (controller, _, _, _) = CreateControllerWithMocks();

        // Act
        var result = controller.InlineCssHell(8);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<InlineCssModel>(viewResult.ViewData["Model"]);
        Assert.True(model.enableChaos);
    }

    [Fact]
    public void InlineCssHell_ChaosAtOrBelow5_DisablesChaosFlag()
    {
        // Arrange
        var (controller, _, _, _) = CreateControllerWithMocks();

        // Act
        var result = controller.InlineCssHell(3);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<InlineCssModel>(viewResult.ViewData["Model"]);
        Assert.False(model.enableChaos);
    }

    // ===================================================================
    // ViewLogicCalculator Tests
    // ===================================================================

    [Fact]
    public void ViewLogicCalculator_ReturnsViewResult()
    {
        // Arrange
        var (controller, _, _, _) = CreateControllerWithMocks();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = controller.ViewLogicCalculator();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void ViewLogicCalculator_SetsViewBagData()
    {
        // Arrange
        var (controller, _, _, _) = CreateControllerWithMocks();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = controller.ViewLogicCalculator();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ViewLogicData>(viewResult.ViewData["Data"]);
    }
}
