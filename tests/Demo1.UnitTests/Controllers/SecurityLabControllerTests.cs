using Demo1.Controllers;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Controllers;

public class SecurityLabControllerTests
{
    private static readonly Dictionary<string, bool> DefaultHeaderStates = new()
    {
        ["Content-Security-Policy"] = true,
        ["X-Frame-Options"] = true,
        ["X-Content-Type-Options"] = true,
        ["X-XSS-Protection"] = true,
        ["Referrer-Policy"] = true
    };

    private static readonly List<AttackScenario> DefaultScenarios = new()
    {
        new() { Name = "Cross-Site Scripting (XSS)", Type = AttackType.XSS, AffectedHeader = "Content-Security-Policy" },
        new() { Name = "Clickjacking", Type = AttackType.Clickjacking, AffectedHeader = "X-Frame-Options" },
        new() { Name = "MIME Type Sniffing", Type = AttackType.MimeSniff, AffectedHeader = "X-Content-Type-Options" }
    };

    private static (SecurityLabController controller, Mock<ISecurityLabService> serviceMock) CreateController(
        Mock<ISecurityLabService>? serviceMock = null)
    {
        var mock = serviceMock ?? new Mock<ISecurityLabService>();

        mock.Setup(s => s.GetHeaderStates()).Returns(DefaultHeaderStates);
        mock.Setup(s => s.GetAttackScenarios()).Returns(DefaultScenarios);
        mock.Setup(s => s.GetProtectionScore()).Returns(100);

        var controller = new SecurityLabController(mock.Object, Mock.Of<ILogger<SecurityLabController>>());
        return (controller, mock);
    }

    [Fact]
    public void Index_ReturnsViewWithViewModel()
    {
        // Arrange
        var (controller, _) = CreateController();

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SecurityLabViewModel>(viewResult.Model);
        Assert.Equal(5, model.HeaderStates.Count);
        Assert.Equal(3, model.AttackScenarios.Count);
        Assert.Equal(100, model.ProtectionScore);
    }

    [Fact]
    public void Configure_ReturnsBadRequest_WhenHeaderIsNull()
    {
        // Arrange
        var (controller, _) = CreateController();
        var request = new HeaderConfigRequest { Header = null!, Enabled = true };

        // Act
        var result = controller.Configure(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Configure_ReturnsBadRequest_WhenHeaderIsEmpty()
    {
        // Arrange
        var (controller, _) = CreateController();
        var request = new HeaderConfigRequest { Header = "  ", Enabled = true };

        // Act
        var result = controller.Configure(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Configure_ReturnsBadRequest_WhenRequestIsNull()
    {
        // Arrange
        var (controller, _) = CreateController();

        // Act
        var result = controller.Configure(null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Configure_ReturnsJson_WithUpdatedScore()
    {
        // Arrange
        var mock = new Mock<ISecurityLabService>();
        mock.Setup(s => s.GetHeaderStates()).Returns(DefaultHeaderStates);
        mock.Setup(s => s.GetAttackScenarios()).Returns(DefaultScenarios);
        mock.Setup(s => s.GetProtectionScore()).Returns(80);

        var (controller, _) = CreateController(mock);
        var request = new HeaderConfigRequest { Header = "X-Frame-Options", Enabled = false };

        // Act
        var result = controller.Configure(request);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);

        // Verify service was called
        mock.Verify(s => s.SetHeaderState("X-Frame-Options", false), Times.Once);
        mock.Verify(s => s.GetProtectionScore(), Times.Once);
        mock.Verify(s => s.GetHeaderStates(), Times.Once);
    }

    [Fact]
    public void Reset_ReturnsJson_WithDefaultStates()
    {
        // Arrange
        var (controller, mock) = CreateController();

        // Act
        var result = controller.Reset();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);

        // Verify service interactions
        mock.Verify(s => s.ResetToDefaults(), Times.Once);
        mock.Verify(s => s.GetProtectionScore(), Times.Once);
        mock.Verify(s => s.GetHeaderStates(), Times.Once);
    }

    [Fact]
    public void VictimPage_ReturnsView()
    {
        // Arrange
        var (controller, _) = CreateController();

        // Act
        var result = controller.VictimPage();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Attack_ReturnsNotFound_ForInvalidType()
    {
        // Arrange
        var (controller, _) = CreateController();

        // Act
        var result = controller.Attack("NonExistentAttack");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Attack_ReturnsJson_ForValidType()
    {
        // Arrange
        var (controller, _) = CreateController();

        // Act
        var result = controller.Attack("XSS");

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var scenario = Assert.IsType<AttackScenario>(jsonResult.Value);
        Assert.Equal(AttackType.XSS, scenario.Type);
        Assert.Equal("Content-Security-Policy", scenario.AffectedHeader);
    }

    [Theory]
    [InlineData("Clickjacking")]
    [InlineData("MimeSniff")]
    public void Attack_ReturnsJson_ForAllValidTypes(string attackType)
    {
        // Arrange
        var (controller, _) = CreateController();

        // Act
        var result = controller.Attack(attackType);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.IsType<AttackScenario>(jsonResult.Value);
    }
}
