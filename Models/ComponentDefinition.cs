namespace Demo1.Models;

/// <summary>
/// Represents a UI component registered in the showcase catalog.
/// </summary>
/// <param name="Name">The unique name of the component.</param>
/// <param name="Category">The category grouping for the component (e.g., Buttons, Cards).</param>
/// <param name="Description">A human-readable description of what the component demonstrates.</param>
/// <param name="ViewComponentName">The name of the Razor view used to render the component preview.</param>
/// <param name="ExampleMarkup">A representative HTML markup snippet for the component.</param>
public record ComponentDefinition(
    string Name,
    string Category,
    string Description,
    string ViewComponentName,
    string ExampleMarkup);
