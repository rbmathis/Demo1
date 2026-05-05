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
    private static HomeController CreateController(
        ILogger<HomeController>? logger = null,
        IUserProfileService? userProfileService = null)
    {
        return new HomeController(
            logger ?? Mock.Of<ILogger<HomeController>>(),
            Mock.Of<ISearchService>(),
            Mock.Of<IWeatherService>(),
            userProfileService ?? Mock.Of<IUserProfileService>(),
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

    [Fact]
    public async Task GodObjectProfile_Get_Returns_View_Without_Mutating_State()
    {
        var profileService = new Mock<IUserProfileService>();
        profileService.Setup(s => s.GetProfileAsync(It.IsAny<string>()))
            .ReturnsAsync(new UserProfile { Name = "Test User" });
        profileService.Setup(s => s.GetStats()).Returns(new ProfileStats());
        var controller = CreateController(userProfileService: profileService.Object);

        var result = await controller.GodObjectProfile();

        Assert.IsType<ViewResult>(result);
        profileService.Verify(s => s.UpdateFieldAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GodObjectProfile_Post_UpdatesField_And_Redirects()
    {
        var updatedProfile = new UserProfile { Name = "NewName" };
        var profileService = new Mock<IUserProfileService>();
        profileService.Setup(s => s.UpdateFieldAsync("", "Name", "NewName"))
            .ReturnsAsync(updatedProfile);
        var controller = CreateController(userProfileService: profileService.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            controller.ControllerContext.HttpContext,
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

        var result = await controller.GodObjectProfile("Name", "NewName");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(controller.GodObjectProfile), redirect.ActionName);
        profileService.Verify(s => s.UpdateFieldAsync("", "Name", "NewName"), Times.Once);
    }

    [Fact]
    public async Task GodObjectProfile_Post_InvalidField_LogsWarning_And_Redirects()
    {
        var logger = new Mock<ILogger<HomeController>>();
        var profileService = new Mock<IUserProfileService>();
        profileService.Setup(s => s.UpdateFieldAsync("", "BadField", "x"))
            .ThrowsAsync(new ArgumentException("Field 'BadField' is not updatable.", "fieldName"));
        var controller = CreateController(logger: logger.Object, userProfileService: profileService.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            controller.ControllerContext.HttpContext,
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

        var result = await controller.GodObjectProfile("BadField", "x");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(controller.GodObjectProfile), redirect.ActionName);
        logger.VerifyLog(LogLevel.Warning, Times.Once());
    }
}