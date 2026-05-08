using Microsoft.AspNetCore.Mvc;

namespace Demo1.ViewComponents;

/// <summary>
/// Renders a showcase of Bootstrap alert variants including contextual colors
/// and dismissible alerts.
/// </summary>
public class AlertShowcaseViewComponent : ViewComponent
{
    /// <summary>
    /// Renders the alert showcase component.
    /// </summary>
    /// <returns>The default view for the component.</returns>
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
