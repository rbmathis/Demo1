using Demo1.Controllers;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Controllers;

/// <summary>
/// Unit tests for <see cref="ComponentShowcaseController"/> verifying Index and Preview actions.
/// </summary>
public class ComponentShowcaseControllerTests
{
    private readonly Mock<IComponentRegistryService> _registryServiceMock;
    private readonly Mock<ILogger<ComponentShowcaseController>> _loggerMock;
    private readonly ComponentShowcaseController _controller;

    public ComponentShowcaseControllerTests()
    {
        _registryServiceMock = new Mock<IComponentRegistryService>();
        _loggerMock = new Mock<ILogger<ComponentShowcaseController>>();
        _controller = new ComponentShowcaseController(_registryServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Index_Returns_ViewResult_With_AllComponents()
    {
        // Arrange
        var components = new List<ComponentDefinition>
        {
            new("ButtonShowcase", "Buttons", "Bootstrap button variants", "ButtonShowcase", "<button class=\"btn btn-primary\">Primary</button>"),
            new("CardShowcase", "Cards", "Card layouts", "CardShowcase", "<div class=\"card\"><div class=\"card-body\">Card</div></div>"),
            new("AlertShowcase", "Alerts", "Alert messages", "AlertShowcase", "<div class=\"alert alert-success\">Success</div>"),
            new("FormShowcase", "Forms", "Form elements", "FormShowcase", "<input type=\"text\" class=\"form-control\" placeholder=\"Text input\" />"),
            new("BadgeShowcase", "Badges", "Badge variants", "BadgeShowcase", "<span class=\"badge bg-primary\">Badge</span>")
        };
        _registryServiceMock.Setup(s => s.GetAll()).Returns(components);

        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<ComponentDefinition>>(viewResult.Model);
        Assert.Equal(5, model.Count());
    }

    [Fact]
    public void Preview_ValidName_Returns_ViewResult_With_Component()
    {
        // Arrange
        var component = new ComponentDefinition(
            "ButtonShowcase", "Buttons", "Bootstrap button variants",
            "ButtonShowcase", "<button class=\"btn btn-primary\">Primary</button>");
        _registryServiceMock.Setup(s => s.GetByName("ButtonShowcase")).Returns(component);

        // Act
        var result = _controller.Preview("ButtonShowcase");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ComponentDefinition>(viewResult.Model);
        Assert.Equal("ButtonShowcase", model.Name);
        Assert.Equal("Buttons", model.Category);
    }

    [Fact]
    public void Preview_InvalidName_Returns_NotFound()
    {
        // Arrange
        _registryServiceMock.Setup(s => s.GetByName("NonExistent")).Returns((ComponentDefinition?)null);

        // Act
        var result = _controller.Preview("NonExistent");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Preview_NullOrEmpty_Returns_BadRequest(string? name)
    {
        // Act
        var result = _controller.Preview(name!);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }
}
