using Demo1.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Controllers;

/// <summary>
/// Unit tests for <see cref="AdminAuthController"/>.
/// </summary>
public class AdminAuthControllerTests
{
    private static AdminAuthController CreateController(
        IConfiguration? config = null,
        ILogger<AdminAuthController>? logger = null)
    {
        config ??= new ConfigurationBuilder().Build();
        logger ??= Mock.Of<ILogger<AdminAuthController>>();
        var controller = new AdminAuthController(config, logger);

        // Build a service provider that satisfies the MVC internals needed by View() calls
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IAuthenticationService>());

        // Register ITempDataDictionaryFactory so Controller.View() can resolve TempData
        var mockTempData = Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary>();
        var mockTempDataFactory = new Mock<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory>();
        mockTempDataFactory
            .Setup(f => f.GetTempData(It.IsAny<HttpContext>()))
            .Returns(mockTempData);
        services.AddSingleton(mockTempDataFactory.Object);

        // Register IModelMetadataProvider so Controller.ViewData can be constructed
        services.AddSingleton(Mock.Of<Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider>());

        var serviceProvider = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Wire up a URL helper so Url.IsLocalUrl works
        controller.Url = Mock.Of<IUrlHelper>(u =>
            u.IsLocalUrl(It.IsAny<string>()) == false);

        return controller;
    }

    [Fact]
    public void Get_Login_ReturnsView()
    {
        var controller = CreateController();

        var result = controller.Login(returnUrl: null);

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Post_Login_WhenPasswordNotConfigured_ReturnsViewWithError()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("AdminDashboard:Username", "admin"),
                new("AdminDashboard:Password", ""),  // empty → not configured
            ])
            .Build();

        var controller = CreateController(config: config);

        var result = await controller.Login("admin", "anything", returnUrl: null);

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Post_Login_WithWrongPassword_ReturnsViewWithError()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("AdminDashboard:Username", "admin"),
                new("AdminDashboard:Password", "correct-password"),
            ])
            .Build();

        var controller = CreateController(config: config);

        var result = await controller.Login("admin", "wrong-password", returnUrl: null);

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Post_Login_WithWrongUsername_ReturnsViewWithError()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("AdminDashboard:Username", "admin"),
                new("AdminDashboard:Password", "secret"),
            ])
            .Build();

        var controller = CreateController(config: config);

        var result = await controller.Login("notadmin", "secret", returnUrl: null);

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Post_Login_WithCorrectCredentials_RedirectsToFeatureFlag()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("AdminDashboard:Username", "admin"),
                new("AdminDashboard:Password", "correct"),
            ])
            .Build();

        // SignInAsync needs a real IAuthenticationService mock that accepts calls
        var authService = new Mock<IAuthenticationService>();
        authService
            .Setup(s => s.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string?>(),
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);

        var mockTempData = Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary>();
        var mockTempDataFactory = new Mock<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory>();
        mockTempDataFactory.Setup(f => f.GetTempData(It.IsAny<HttpContext>())).Returns(mockTempData);

        var services = new ServiceCollection();
        services.AddSingleton(authService.Object);
        services.AddSingleton(mockTempDataFactory.Object);
        services.AddSingleton(Mock.Of<Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider>());
        var serviceProvider = services.BuildServiceProvider();

        var controller = new AdminAuthController(config, Mock.Of<ILogger<AdminAuthController>>());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = serviceProvider }
        };
        controller.Url = Mock.Of<IUrlHelper>(u => u.IsLocalUrl(It.IsAny<string>()) == false);

        var result = await controller.Login("admin", "correct", returnUrl: null);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("FeatureFlag", redirect.ControllerName);
    }

    [Fact]
    public async Task Post_Logout_RedirectsToHomeIndex()
    {
        var authService = new Mock<IAuthenticationService>();
        authService
            .Setup(s => s.SignOutAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string?>(),
                It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(authService.Object);
        var serviceProvider = services.BuildServiceProvider();

        var controller = new AdminAuthController(
            new ConfigurationBuilder().Build(),
            Mock.Of<ILogger<AdminAuthController>>());

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = serviceProvider }
        };
        controller.Url = Mock.Of<IUrlHelper>(u => u.IsLocalUrl(It.IsAny<string>()) == false);

        var result = await controller.Logout();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }
}
