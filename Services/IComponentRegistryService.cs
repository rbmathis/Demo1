using Demo1.Models;

namespace Demo1.Services;

/// <summary>
/// Provides access to the catalog of registered UI components for the showcase.
/// </summary>
public interface IComponentRegistryService
{
    /// <summary>
    /// Gets all registered component definitions.
    /// </summary>
    /// <returns>An enumerable of all component definitions.</returns>
    IEnumerable<ComponentDefinition> GetAll();

    /// <summary>
    /// Gets component definitions filtered by category.
    /// Future: Used by category filter feature.
    /// </summary>
    /// <param name="category">The category name to filter by (case-insensitive).</param>
    /// <returns>An enumerable of matching component definitions.</returns>
    IEnumerable<ComponentDefinition> GetByCategory(string category);

    /// <summary>
    /// Gets a single component definition by its unique name.
    /// </summary>
    /// <param name="name">The component name to look up (case-insensitive).</param>
    /// <returns>The matching component definition, or null if not found.</returns>
    ComponentDefinition? GetByName(string name);
}
