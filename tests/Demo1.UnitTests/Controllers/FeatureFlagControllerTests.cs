using Demo1.Controllers;
using Demo1.Models;
using Demo1.Services;
using Demo1.UnitTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace Demo1.UnitTests.Controllers;

/// <summary>
/// Unit tests for <see cref="FeatureFlagController"/>.
/// </summary>
public class FeatureFlagControllerTests
{
    private static FeatureFlagController CreateController(
        IFeatureFlagService? service = null,
        ILogger<FeatureFlagController>? logger = null,
        string? userName = "admin")
    {
        var svc = service ?? Mock.Of<IFeatureFlagService>();
        var log = logger ?? Mock.Of<ILogger<FeatureFlagController>>();
        var controller = new FeatureFlagController(svc, log);

        // Wire up a fake identity so User.Identity.Name resolves
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName ?? "admin"),
            new(ClaimTypes.Role, "Admin"),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Wire up TempData so the controller can write success/error messages
        controller.TempData = new TempDataDictionary(
            controller.HttpContext,
            Mock.Of<ITempDataProvider>());

        return controller;
    }

    // ── Index ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Index_ReturnsViewResult()
    {
        var mockSvc = new Mock<IFeatureFlagService>();
        mockSvc.Setup(s => s.GetFlagsAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(Array.Empty<FeatureFlagViewModel>());

        var controller = CreateController(service: mockSvc.Object);

        var result = await controller.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Index_PassesDashboardViewModelToView()
    {
        var flags = new List<FeatureFlagViewModel>
        {
            new() { Name = "Feature1", IsEnabled = true, Source = "Local config", Label = "" }
        };

        var mockSvc = new Mock<IFeatureFlagService>();
        mockSvc.Setup(s => s.GetFlagsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(flags);
        mockSvc.Setup(s => s.IsAzureAppConfigurationAvailable).Returns(false);

        var controller = CreateController(service: mockSvc.Object);

        var result = (ViewResult)await controller.Index();
        var model = Assert.IsType<FeatureFlagDashboardViewModel>(result.Model);

        Assert.Single(model.Flags);
        Assert.False(model.IsAzureAppConfigurationAvailable);
    }

    [Fact]
    public async Task Index_SetsIsAzureAvailable_FromService()
    {
        var mockSvc = new Mock<IFeatureFlagService>();
        mockSvc.Setup(s => s.GetFlagsAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(Array.Empty<FeatureFlagViewModel>());
        mockSvc.Setup(s => s.IsAzureAppConfigurationAvailable).Returns(true);

        var controller = CreateController(service: mockSvc.Object);

        var result = (ViewResult)await controller.Index();
        var model = Assert.IsType<FeatureFlagDashboardViewModel>(result.Model);

        Assert.True(model.IsAzureAppConfigurationAvailable);
    }

    // ── Toggle ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Toggle_RedirectsToIndex_OnSuccess()
    {
        var mockSvc = new Mock<IFeatureFlagService>();
        mockSvc.Setup(s => s.SetFlagAsync("Feature1", true, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

        var controller = CreateController(service: mockSvc.Object);

        var result = await controller.Toggle("Feature1", enabled: true);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Toggle_RedirectsToIndex_OnFailure()
    {
        var mockSvc = new Mock<IFeatureFlagService>();
        mockSvc.Setup(s => s.SetFlagAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);

        var controller = CreateController(service: mockSvc.Object);

        var result = await controller.Toggle("Feature1", enabled: true);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Toggle_SetsTempDataSuccess_OnSuccess()
    {
        var mockSvc = new Mock<IFeatureFlagService>();
        mockSvc.Setup(s => s.SetFlagAsync("DarkMode", false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

        var controller = CreateController(service: mockSvc.Object);

        await controller.Toggle("DarkMode", enabled: false);

        Assert.True(controller.TempData.ContainsKey("Success"));
    }

    [Fact]
    public async Task Toggle_SetsTempDataError_OnFailure()
    {
        var mockSvc = new Mock<IFeatureFlagService>();
        mockSvc.Setup(s => s.SetFlagAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);
        mockSvc.Setup(s => s.IsAzureAppConfigurationAvailable).Returns(false);

        var controller = CreateController(service: mockSvc.Object);

        await controller.Toggle("Feature1", enabled: true);

        Assert.True(controller.TempData.ContainsKey("Error"));
    }

    [Fact]
    public async Task Toggle_LogsInformation_OnRequest()
    {
        var logger = new Mock<ILogger<FeatureFlagController>>();
        var mockSvc = new Mock<IFeatureFlagService>();
        mockSvc.Setup(s => s.SetFlagAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

        var controller = CreateController(service: mockSvc.Object, logger: logger.Object);

        await controller.Toggle("Feature1", enabled: true);

        logger.VerifyLog(LogLevel.Information, Times.AtLeastOnce());
    }
}
