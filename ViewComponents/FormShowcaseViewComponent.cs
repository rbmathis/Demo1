using Microsoft.AspNetCore.Mvc;

namespace Demo1.ViewComponents;

/// <summary>
/// Renders a showcase of Bootstrap form elements including text inputs,
/// selects, checkboxes, radios, and textareas.
/// </summary>
public class FormShowcaseViewComponent : ViewComponent
{
    /// <summary>
    /// Renders the form showcase component.
    /// </summary>
    /// <returns>The default view for the component.</returns>
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
