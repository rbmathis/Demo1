using Demo1.Models;

namespace Demo1.Services;

/// <summary>
/// Singleton implementation of <see cref="IComponentRegistryService"/> that maintains
/// a hardcoded registry of UI components for the showcase catalog.
/// </summary>
public class ComponentRegistryService : IComponentRegistryService
{
    private readonly IReadOnlyList<ComponentDefinition> _components;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentRegistryService"/> class
    /// with the default set of showcase components.
    /// </summary>
    public ComponentRegistryService()
    {
        _components = new List<ComponentDefinition>
        {
            new("ButtonShowcase", "Buttons",
                "Bootstrap button variants including primary, secondary, success, danger, outline, and size options.",
                "ButtonShowcase",
                """<button class="btn btn-primary">Primary</button>"""),

            new("CardShowcase", "Cards",
                "Card layouts with headers, footers, images, and horizontal variants.",
                "CardShowcase",
                """<div class="card"><div class="card-body">Card</div></div>"""),

            new("AlertShowcase", "Alerts",
                "Alert messages for success, info, warning, and danger with dismissible options.",
                "AlertShowcase",
                """<div class="alert alert-success">Success</div>"""),

            new("FormShowcase", "Forms",
                "Form elements including text inputs, selects, checkboxes, radios, and textareas.",
                "FormShowcase",
                """<input type="text" class="form-control" placeholder="Text input" />"""),

            new("BadgeShowcase", "Badges",
                "Badge variants including contextual colors, pills, and notification badges.",
                "BadgeShowcase",
                """<span class="badge bg-primary">Badge</span>""")
        }.AsReadOnly();
    }

    /// <inheritdoc />
    public IEnumerable<ComponentDefinition> GetAll()
    {
        return _components;
    }

    /// <inheritdoc />
    // Future: Used by category filter feature
    public IEnumerable<ComponentDefinition> GetByCategory(string category)
    {
        return _components
            .Where(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <inheritdoc />
    public ComponentDefinition? GetByName(string name)
    {
        return _components
            .FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
