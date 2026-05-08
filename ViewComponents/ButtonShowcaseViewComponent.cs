using Microsoft.AspNetCore.Mvc;

namespace Demo1.ViewComponents;

/// <summary>
/// Renders a showcase of Bootstrap button variants including contextual colors,
/// outlines, and size options.
/// </summary>
public class ButtonShowcaseViewComponent : ViewComponent
{
    /// <summary>
    /// Renders the button showcase component.
    /// </summary>
    /// <returns>The default view for the component.</returns>
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
