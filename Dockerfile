# syntax=docker/dockerfile:1
# Demo1 ASP.NET Core MVC Application
# Multi-stage build for optimized image size

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install LibMan for client-side library management
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli

# Add .NET tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy csproj and restore dependencies
COPY Demo1.csproj ./
RUN dotnet restore

# Copy everything else
COPY . ./

# Restore client-side libraries
RUN libman restore

# Build and publish
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health check
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r demo1 && useradd -r -g demo1 demo1

# Copy published app
COPY --from=build /app/publish .

# Set proper permissions
RUN chown -R demo1:demo1 /app

# Switch to non-root user
USER demo1

# Expose port (configurable via environment)
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
  ASPNETCORE_ENVIRONMENT=Production \
  DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "Demo1.dll"]
