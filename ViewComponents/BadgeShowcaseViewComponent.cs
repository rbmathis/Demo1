using Microsoft.AspNetCore.Mvc;

namespace Demo1.ViewComponents;

/// <summary>
/// Renders a showcase of Bootstrap badge variants including contextual colors,
/// pill badges, and notification badge patterns.
/// </summary>
public class BadgeShowcaseViewComponent : ViewComponent
{
    /// <summary>
    /// Renders the badge showcase component.
    /// </summary>
    /// <returns>The default view for the component.</returns>
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
