using Demo1.Controllers;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Controllers;

/// <summary>
/// Tests for the profile management actions in ProfileController.
/// </summary>
public class ProfileControllerTests
{
    private static ProfileController CreateController(
        IUserProfileService userProfileService,
        ILogger<ProfileController>? logger = null)
    {
        var controller = new ProfileController(
            logger ?? Mock.Of<ILogger<ProfileController>>(),
            userProfileService);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.TempData = new TempDataDictionary(
            controller.HttpContext,
            Mock.Of<ITempDataProvider>());

        return controller;
    }

    [Fact]
    public async Task GodObjectProfile_Get_DoesNotMutateState()
    {
        var userProfileService = new Mock<IUserProfileService>();
        userProfileService
            .Setup(service => service.GetProfileAsync(""))
            .ReturnsAsync(new UserProfile());
        userProfileService
            .Setup(service => service.GetStats())
            .Returns(new ProfileStats());

        var controller = CreateController(userProfileService.Object);

        var result = await controller.GodObjectProfile("update", "Name", "Mallory");

        Assert.IsType<ViewResult>(result);
        userProfileService.Verify(service => service.UpdateFieldAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GodObjectProfileUpdate_Post_WithValidData_CallsService()
    {
        var userProfileService = new Mock<IUserProfileService>();
        userProfileService
            .Setup(service => service.UpdateFieldAsync("", "Name", "Alice"))
            .ReturnsAsync(new UserProfile { Name = "Alice" });

        var controller = CreateController(userProfileService.Object);

        await controller.GodObjectProfileUpdate("Name", "Alice");

        userProfileService.Verify(service => service.UpdateFieldAsync("", "Name", "Alice"), Times.Once);
    }

    [Fact]
    public async Task GodObjectProfileUpdate_Post_WithInvalidField_SetsError()
    {
        var exception = new ArgumentException("Field 'Password' is not updatable.");
        var logger = new Mock<ILogger<ProfileController>>();
        var userProfileService = new Mock<IUserProfileService>();
        userProfileService
            .Setup(service => service.UpdateFieldAsync("", "Password", "secret"))
            .ThrowsAsync(exception);

        var controller = CreateController(userProfileService.Object, logger.Object);

        await controller.GodObjectProfileUpdate("Password", "secret");

        Assert.Equal(exception.Message, controller.TempData["Error"]);
    }

    [Fact]
    public async Task GodObjectProfileUpdate_Post_RedirectsAfterSuccess()
    {
        var userProfileService = new Mock<IUserProfileService>();
        userProfileService
            .Setup(service => service.UpdateFieldAsync("", "Name", "Alice"))
            .ReturnsAsync(new UserProfile { Name = "Alice" });

        var controller = CreateController(userProfileService.Object);

        var result = await controller.GodObjectProfileUpdate("Name", "Alice");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProfileController.GodObjectProfile), redirect.ActionName);
    }
}
