---
applyTo: "**/*.csproj"
---

# Project File Instructions

## .NET Project Guidelines

- Target the latest LTS or current .NET version (currently .NET 9)
- Enable nullable reference types (`<Nullable>enable</Nullable>`)
- Enable implicit usings (`<ImplicitUsings>enable</ImplicitUsings>`)
- Generate XML documentation (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`)

## Package Management

- Keep NuGet packages up to date
- Use specific version numbers (avoid floating versions)
- Prefer Microsoft-supported packages when available
- Run security scans on dependencies regularly

## Build Configuration

- Use Release configuration for production builds
- Configure appropriate warning levels
- Enable code analysis where appropriate
