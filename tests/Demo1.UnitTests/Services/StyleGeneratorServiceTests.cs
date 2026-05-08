using Demo1.Services;

namespace Demo1.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="StyleGeneratorService"/> verifying random font, color, and chaos style generation.
/// </summary>
public class StyleGeneratorServiceTests
{
    private readonly StyleGeneratorService _service;

    private static readonly string[] KnownFonts =
    {
        "Comic Sans MS",
        "Papyrus",
        "Impact",
        "Courier New",
        "Arial Black",
        "Times New Roman",
        "Verdana",
        "Georgia",
    };

    public StyleGeneratorServiceTests()
    {
        _service = new StyleGeneratorService();
    }

    /// <summary>
    /// Verifies that GetRandomFont returns a non-null, non-empty string.
    /// </summary>
    [Fact]
    public void GetRandomFont_ReturnsNonEmptyString()
    {
        // Arrange & Act
        var font = _service.GetRandomFont();

        // Assert
        Assert.NotNull(font);
        Assert.NotEmpty(font);
    }

    /// <summary>
    /// Verifies that GetRandomFont returns one of the known font families.
    /// </summary>
    [Fact]
    public void GetRandomFont_ReturnsKnownFont()
    {
        // Arrange & Act
        var font = _service.GetRandomFont();

        // Assert
        Assert.Contains(font, KnownFonts);
    }

    /// <summary>
    /// Verifies that GetRandomColor returns a non-null, non-empty string.
    /// </summary>
    [Fact]
    public void GetRandomColor_ReturnsNonEmptyString()
    {
        // Arrange & Act
        var color = _service.GetRandomColor();

        // Assert
        Assert.NotNull(color);
        Assert.NotEmpty(color);
    }

    /// <summary>
    /// Verifies that GetRandomColor returns a valid hex color string (e.g., "#FF6B6B").
    /// </summary>
    [Fact]
    public void GetRandomColor_ReturnsHexColor()
    {
        // Arrange & Act
        var color = _service.GetRandomColor();

        // Assert
        Assert.StartsWith("#", color);
        Assert.Equal(7, color.Length);
    }

    /// <summary>
    /// Verifies that GenerateChaosStyle returns a non-null, non-empty string.
    /// </summary>
    [Fact]
    public void GenerateChaosStyle_ReturnsNonEmptyString()
    {
        // Arrange & Act
        var style = _service.GenerateChaosStyle();

        // Assert
        Assert.NotNull(style);
        Assert.NotEmpty(style);
    }

    /// <summary>
    /// Verifies that GenerateChaosStyle contains CSS property syntax (colon and semicolon separators).
    /// </summary>
    [Fact]
    public void GenerateChaosStyle_ContainsCssProperties()
    {
        // Arrange & Act
        var style = _service.GenerateChaosStyle();

        // Assert
        Assert.Contains(":", style);
        Assert.Contains(";", style);
    }

    /// <summary>
    /// Verifies that at least one generated chaos style contains a font-family declaration.
    /// </summary>
    [Fact]
    public void GenerateChaosStyle_ContainsFontFamily()
    {
        // Arrange
        var containsFontFamily = false;

        // Act
        for (var i = 0; i < 50; i++)
        {
            var style = _service.GenerateChaosStyle();
            if (style.Contains("font-family"))
            {
                containsFontFamily = true;
                break;
            }
        }

        // Assert
        Assert.True(containsFontFamily, "Expected at least one generated style to contain 'font-family' within 50 attempts.");
    }

    /// <summary>
    /// Verifies that GenerateChaosStyle produces varied output across multiple calls (randomness check).
    /// </summary>
    [Fact]
    public void GenerateChaosStyle_ProducesVariedOutput()
    {
        // Arrange & Act
        var styles = Enumerable.Range(0, 10)
            .Select(_ => _service.GenerateChaosStyle())
            .ToHashSet();

        // Assert
        Assert.True(styles.Count > 1, "Expected varied output from 10 calls to GenerateChaosStyle, but all were identical.");
    }
}
