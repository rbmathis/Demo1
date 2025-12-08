# Swagger / OpenAPI (Optional)

This page shows how to enable Swagger (OpenAPI) for local development and optional API documentation publishing.

## Why Swagger?

- Provides interactive API documentation at `/swagger`.
- Makes it easy to try endpoints and share API contracts with other teams.

## Minimal setup (non-invasive)

Add the following in `Program.cs` near service registrations (before `builder.Build()`):

```csharp
// Add API explorer and Swagger generation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Optionally include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});
```

Then enable middleware in the request pipeline (after `var app = builder.Build();`):

```csharp
// Enable Swagger in Development or when explicitly enabled
var enableSwagger = app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger", false);
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo1 API v1");
    });
}
```

## Configuration

Add to `appsettings.Development.json` to enable locally by default:

```json
{
  "EnableSwagger": true
}
```

Or set environment variable:

```bash
export EnableSwagger=true
```

## Publishing API docs

If you want to publish generated OpenAPI docs as part of CI, generate the OpenAPI JSON and upload it as an artifact or commit it to a docs site. Keep in mind not to expose private or internal endpoints publicly.

## Notes

- Swagger is optional and disabled by default in production for security. Use feature flags or environment guards when enabling in non-development environments.
- Including XML comments provides richer endpoint descriptions.
