# API Versioning Strategy

This project uses **URL-based API versioning** so versions are explicit and easy to consume.

## URL-Based Versioning

Use a version segment directly in the route:

- ` /api/v1/... `
- ` /api/v2/... `

Example route template in controllers:

```csharp
[Route("api/v1/[controller]")]
```

When introducing v2, create a v2 controller route:

```csharp
[Route("api/v2/[controller]")]
```

## Adding a New API Version

When adding a new version (for example, v3), keep changes isolated per version.

1. **Controller**
   - Create a new versioned controller (for example, `Controllers/Api/V3/ProductsController.cs`).
   - Keep v1/v2 controllers in place for backward compatibility.
   - Prefer explicit routes such as `api/v3/products`.

2. **Models**
   - Reuse existing models when contract shape is unchanged.
   - Create version-specific request/response models when the contract changes (for example, `Models/Api/V3/ProductResponseV3.cs`).
   - Avoid breaking existing model contracts used by earlier versions.

3. **`Program.cs` considerations**
   - Ensure controller discovery remains enabled (`AddControllers` / `AddControllersWithViews`).
   - If Swagger is enabled, register a Swagger document per API version and a UI endpoint per version (see below).
   - Keep versioning behavior explicit in route design and documentation.

## Deprecation Guidance

Use a predictable deprecation policy:

- **Announce first**: mark the older version as deprecated in release notes/docs.
- **Provide overlap**: keep old and new versions running during a transition window.
- **Communicate timeline**: publish a clear sunset date.
- **Remove last**: only remove deprecated versions after the communicated date.

Recommended minimum policy:

- Support at least one previous major API version.
- Provide at least one release cycle of overlap before removal.

## Example Endpoint Calls

Assume a `products` endpoint:

### v1

```bash
curl https://localhost:5001/api/v1/products
```

### v2

```bash
curl https://localhost:5001/api/v2/products
```

## Swagger / OpenAPI Versioned Docs

If Swagger is enabled, expose one document per API version:

- `/swagger/v1/swagger.json`
- `/swagger/v2/swagger.json`

Swagger UI can publish both:

- `Demo1 API v1`
- `Demo1 API v2`

See [swagger.md](swagger.md) for the base Swagger setup.
