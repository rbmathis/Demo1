# Component Showcase

The Component Showcase is a browsable catalog of UI components with isolated previews, copy-to-clipboard markup, and category filtering.

## Overview

Navigate to `/ComponentShowcase` to browse all registered components. Each component is displayed in an isolated iframe preview, with a description and a copyable markup snippet.

## Architecture

### Controller

`ComponentShowcaseController` (`Controllers/ComponentShowcaseController.cs`) exposes two routes:

| Route | Action | Description |
|-------|--------|-------------|
| `GET /ComponentShowcase` | `Index` | Lists all components in a tabbed interface grouped by category |
| `GET /ComponentShowcase/Preview/{name}` | `Preview` | Renders a single component in a bare layout (used by iframes) |

### Service

`IComponentRegistryService` / `ComponentRegistryService` — a singleton service providing a hardcoded registry of component definitions. Methods:

- `GetAll()` — returns all registered components
- `GetByCategory(string category)` — filters by category (case-insensitive)
- `GetByName(string name)` — finds a single component by name (case-insensitive)

### Model

`ComponentDefinition` is an immutable record:

```csharp
public record ComponentDefinition(
    string Name,
    string Category,
    string Description,
    string ViewComponentName,
    string ExampleMarkup);
```

### ViewComponents

Each category has a ViewComponent for isolated rendering:

| Component | Category | Description |
|-----------|----------|-------------|
| `ButtonShowcaseViewComponent` | Buttons | Bootstrap button variants (contextual, outline, sizes) |
| `CardShowcaseViewComponent` | Cards | Card layouts (basic, header/footer, list group) |
| `AlertShowcaseViewComponent` | Alerts | Alert variants (contextual, dismissible) |
| `FormShowcaseViewComponent` | Forms | Form elements (inputs, selects, checkboxes, radios) |
| `BadgeShowcaseViewComponent` | Badges | Badge variants (contextual, pill, notification) |

### Tag Helper

`CopyMarkupTagHelper` targets `<copy-markup>` elements. It:
1. Reads child HTML content
2. HTML-encodes it for safe display in `<pre><code>` blocks
3. Adds a "Copy" button with `data-clipboard-text` for clipboard functionality

### Assets

- `wwwroot/js/component-showcase.js` — copy-to-clipboard handler and iframe theme sync
- `wwwroot/css/component-showcase.css` — iframe sizing, code block styling, copy button positioning

## Adding New Components

1. Create a ViewComponent class in `ViewComponents/`
2. Create its view in `Views/Shared/Components/{Name}/Default.cshtml`
3. Add a `ComponentDefinition` entry in `ComponentRegistryService`
4. The component will automatically appear in the showcase

## Testing

- **Unit tests:** `ComponentShowcaseControllerTests`, `ComponentRegistryServiceTests`, `CopyMarkupTagHelperTests`
- **Integration tests:** `ComponentShowcaseControllerTests` (verifies routes return expected status codes)
