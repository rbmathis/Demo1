using Demo1.Models;
using Demo1.Services;

namespace Demo1.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="ComponentRegistryService"/> verifying component lookup, filtering, and case-insensitive matching.
/// </summary>
public class ComponentRegistryServiceTests
{
    private readonly ComponentRegistryService _service;

    public ComponentRegistryServiceTests()
    {
        _service = new ComponentRegistryService();
    }

    /// <summary>
    /// Verifies that GetAll returns all five registered components.
    /// </summary>
    [Fact]
    public void GetAll_Returns_AllComponents()
    {
        // Act
        var result = _service.GetAll();

        // Assert
        Assert.Equal(5, result.Count());
    }

    /// <summary>
    /// Verifies that GetByCategory returns only components matching the specified category.
    /// </summary>
    [Fact]
    public void GetByCategory_ValidCategory_Returns_FilteredComponents()
    {
        // Act
        var result = _service.GetByCategory("Buttons");

        // Assert
        Assert.Single(result);
        Assert.Equal("ButtonShowcase", result.First().Name);
    }

    /// <summary>
    /// Verifies that GetByCategory returns an empty collection for a non-existent category.
    /// </summary>
    [Fact]
    public void GetByCategory_InvalidCategory_Returns_Empty()
    {
        // Act
        var result = _service.GetByCategory("NonExistent");

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that GetByCategory performs case-insensitive matching.
    /// </summary>
    [Fact]
    public void GetByCategory_IsCaseInsensitive()
    {
        // Act
        var result = _service.GetByCategory("buttons");

        // Assert
        Assert.Single(result);
        Assert.Equal("ButtonShowcase", result.First().Name);
    }

    /// <summary>
    /// Verifies that GetByName returns the correct component for a valid name.
    /// </summary>
    [Fact]
    public void GetByName_ValidName_Returns_Component()
    {
        // Act
        var result = _service.GetByName("ButtonShowcase");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ButtonShowcase", result.Name);
        Assert.Equal("Buttons", result.Category);
    }

    /// <summary>
    /// Verifies that GetByName returns null for a non-existent component name.
    /// </summary>
    [Fact]
    public void GetByName_InvalidName_Returns_Null()
    {
        // Act
        var result = _service.GetByName("NonExistent");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that GetByName performs case-insensitive matching.
    /// </summary>
    [Fact]
    public void GetByName_IsCaseInsensitive()
    {
        // Act
        var result = _service.GetByName("buttonshowcase");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ButtonShowcase", result.Name);
    }
}
