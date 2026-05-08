using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Demo1.TagHelpers;

/// <summary>
/// Tag helper that renders a <c>&lt;copy-markup&gt;</c> element as a container
/// with a formatted code preview and a one-click copy button.
/// </summary>
[HtmlTargetElement("copy-markup")]
public class CopyMarkupTagHelper : TagHelper
{
    /// <summary>
    /// Processes the <c>&lt;copy-markup&gt;</c> tag, rendering a code preview
    /// block with an HTML-encoded snippet and a copy-to-clipboard button.
    /// </summary>
    /// <param name="context">The tag helper context.</param>
    /// <param name="output">The tag helper output.</param>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var childContent = await output.GetChildContentAsync();
        var rawContent = childContent.GetContent();
        var encodedContent = HtmlEncoder.Default.Encode(rawContent);

        output.TagName = "div";
        output.Attributes.SetAttribute("class", "copy-markup-container");

        output.Content.SetHtmlContent(
            $"""
            <pre><code>{encodedContent}</code></pre>
            <button class="btn btn-sm btn-outline-secondary copy-btn" type="button" data-clipboard-text="{HtmlEncoder.Default.Encode(rawContent)}">Copy</button>
            """);
    }
}
