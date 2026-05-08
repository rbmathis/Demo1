using Demo1.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Demo1.UnitTests.TagHelpers;

/// <summary>
/// Unit tests for <see cref="CopyMarkupTagHelper"/> verifying HTML encoding, output structure, and copy button rendering.
/// </summary>
public class CopyMarkupTagHelperTests
{
    private readonly CopyMarkupTagHelper _tagHelper;

    public CopyMarkupTagHelperTests()
    {
        _tagHelper = new CopyMarkupTagHelper();
    }

    private static (TagHelperContext context, TagHelperOutput output) CreateContextAndOutput(string childContent)
    {
        var context = new TagHelperContext(
            tagName: "copy-markup",
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: "test");

        var output = new TagHelperOutput(
            "copy-markup",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetHtmlContent(childContent);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });

        return (context, output);
    }

    /// <summary>
    /// Verifies that the tag helper renders a code block with pre and code tags.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Renders_CodeBlock()
    {
        // Arrange
        var (context, output) = CreateContextAndOutput("<button>Click me</button>");

        // Act
        await _tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("<pre><code>", content);
        Assert.Contains("</code></pre>", content);
    }

    /// <summary>
    /// Verifies that the tag helper HTML-encodes the child content for safe display.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_HtmlEncodes_Content()
    {
        // Arrange
        var (context, output) = CreateContextAndOutput("<div class=\"test\">Hello</div>");

        // Act
        await _tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("&lt;div class=&quot;test&quot;&gt;Hello&lt;/div&gt;", content);
    }

    /// <summary>
    /// Verifies that the tag helper includes a copy button with the correct CSS class.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Includes_CopyButton()
    {
        // Arrange
        var (context, output) = CreateContextAndOutput("<span>Test</span>");

        // Act
        await _tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("copy-btn", content);
        Assert.Contains("<button", content);
        Assert.Contains("data-clipboard-text=", content);
    }

    /// <summary>
    /// Verifies that the tag helper changes the output tag name to div.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Sets_TagName_To_Div()
    {
        // Arrange
        var (context, output) = CreateContextAndOutput("<p>Content</p>");

        // Act
        await _tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("div", output.TagName);
    }

    /// <summary>
    /// Verifies that the tag helper sets the container CSS class on the output element.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_Sets_Container_Class()
    {
        // Arrange
        var (context, output) = CreateContextAndOutput("<p>Content</p>");

        // Act
        await _tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.True(output.Attributes.ContainsName("class"));
        Assert.Equal("copy-markup-container", output.Attributes["class"].Value);
    }
}
