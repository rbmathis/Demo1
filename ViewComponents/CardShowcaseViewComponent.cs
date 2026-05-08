using Microsoft.AspNetCore.Mvc;

namespace Demo1.ViewComponents;

/// <summary>
/// Renders a showcase of Bootstrap card layouts including basic cards,
/// cards with headers and footers, and list group variants.
/// </summary>
public class CardShowcaseViewComponent : ViewComponent
{
    /// <summary>
    /// Renders the card showcase component.
    /// </summary>
    /// <returns>The default view for the component.</returns>
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
