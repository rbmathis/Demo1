using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demo1.Controllers;

/// <summary>
/// Controller for the Component Showcase feature, providing a browsable
/// catalog of UI components with isolated previews.
/// </summary>
[Route("[controller]")]
public class ComponentShowcaseController : Controller
{
    /// <summary>
    /// Allow-list of known ViewComponent names that may be dynamically invoked.
    /// Prevents arbitrary component invocation if the registry is ever externalized.
    /// </summary>
    private static readonly HashSet<string> AllowedViewComponents = new(StringComparer.OrdinalIgnoreCase)
    {
        "ButtonShowcase",
        "CardShowcase",
        "AlertShowcase",
        "FormShowcase",
        "BadgeShowcase"
    };

    private readonly IComponentRegistryService _registryService;
    private readonly ILogger<ComponentShowcaseController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentShowcaseController"/> class.
    /// </summary>
    /// <param name="registryService">The component registry service.</param>
    /// <param name="logger">The logger instance.</param>
    public ComponentShowcaseController(IComponentRegistryService registryService, ILogger<ComponentShowcaseController> logger)
    {
        _registryService = registryService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the main showcase index listing all registered components.
    /// </summary>
    /// <returns>The index view populated with all component definitions.</returns>
    [HttpGet("")]
    public IActionResult Index()
    {
        var components = _registryService.GetAll();
        return View(components);
    }

    /// <summary>
    /// Displays an isolated preview for a single component by name.
    /// </summary>
    /// <param name="name">The unique name of the component to preview.</param>
    /// <returns>The preview view for the component, or NotFound if the component does not exist.</returns>
    [HttpGet("Preview/{name:alpha:maxlength(50)}")]
    public IActionResult Preview(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest();
        }

        var component = _registryService.GetByName(name);

        if (component is null)
        {
            _logger.LogWarning("Component preview requested for unknown name: {ComponentName}", name);
            return NotFound();
        }

        if (!AllowedViewComponents.Contains(component.ViewComponentName))
        {
            _logger.LogWarning("Component {ComponentName} references disallowed ViewComponent: {ViewComponentName}",
                name, component.ViewComponentName);
            return NotFound();
        }

        _logger.LogDebug("Previewing component {ComponentName} in category {Category}",
            component.Name, component.Category);

        return View(component);
    }
}
