using Demo1.Controllers;
using Demo1.Models;
using Demo1.Services;
using Demo1.UnitTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Controllers;

public class HomeControllerTests
{
    private static HomeController CreateController(ILogger<HomeController>? logger = null)
    {
        return new HomeController(
            logger ?? Mock.Of<ILogger<HomeController>>(),
            Mock.Of<ISearchService>(),
            Mock.Of<IWeatherService>(),
            Mock.Of<IUserProfileService>(),
            Mock.Of<IStyleGeneratorService>()
        );
    }

    [Fact]
    public void Index_Returns_Default_View()
    {
        var controller = CreateController();

        var result = controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Null(view.ViewName);
    }

    [Fact]
    public void Privacy_Returns_Default_View()
    {
        var controller = CreateController();

        var result = controller.Privacy();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void AboutUs_Returns_Default_View()
    {
        var controller = CreateController();

        var result = controller.AboutUs();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Feature1_Returns_View_And_Logs_Access()
    {
        var logger = new Mock<ILogger<HomeController>>();
        var controller = CreateController(logger.Object);

        var result = controller.Feature1();

        Assert.IsType<ViewResult>(result);
        logger.VerifyLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public void Error404_Returns_View_With_RequestId()
    {
        var controller = CreateController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { TraceIdentifier = "trace-404" }
        };

        var result = controller.Error404();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ErrorViewModel>(view.Model);
        Assert.Equal("trace-404", model.RequestId);
    }

    [Fact]
    public void Error500_Returns_View_With_RequestId()
    {
        var controller = CreateController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { TraceIdentifier = "trace-500" }
        };

        var result = controller.Error500();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ErrorViewModel>(view.Model);
        Assert.Equal("trace-500", model.RequestId);
    }

    [Fact]
    public void Error_Returns_View_With_RequestId()
    {
        var controller = CreateController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { TraceIdentifier = "trace-error" }
        };

        var result = controller.Error();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ErrorViewModel>(view.Model);
        Assert.Equal("trace-error", model.RequestId);
    }
}
