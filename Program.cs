using Demo1.Middleware;
using Demo1.Telemetry;
using Demo1.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.FeatureManagement;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();

// ✅ 12-FACTOR: Register application services via dependency injection
builder.Services.AddSingleton<ISearchService, InMemorySearchService>();
builder.Services.AddSingleton<IWeatherService, MockWeatherService>();
builder.Services.AddSingleton<IUserProfileService, InMemoryUserProfileService>();
builder.Services.AddSingleton<IStyleGeneratorService, StyleGeneratorService>();

// ✅ 12-FACTOR: Configure distributed cache based on environment
var cacheProvider = builder.Configuration["CacheProvider"] ?? "Memory";
if (cacheProvider.Equals("Redis", StringComparison.OrdinalIgnoreCase))
{
    var redisConnectionString = builder.Configuration["Redis:ConnectionString"]
        ?? Environment.GetEnvironmentVariable("REDIS_CONNECTIONSTRING")
        ?? "localhost:6379";

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "Demo1_";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".Demo1.Session";
});

// Flag set when Azure App Configuration provider is successfully added
var azureAppConfigRegistered = false;

// ✅ 12-FACTOR: Configuration from environment variables, not hardcoded
var appConfigEndpoint = Environment.GetEnvironmentVariable("AZUREAPPCONFIGURATION__ENDPOINT")
    ?? builder.Configuration["AzureAppConfiguration:Endpoint"];
var appConfigConnectionString = Environment.GetEnvironmentVariable("AZUREAPPCONFIGURATION__CONNECTIONSTRING")
    ?? builder.Configuration["AzureAppConfiguration:ConnectionString"];
var appConfigLabel = builder.Configuration["AzureAppConfiguration:Label"] ?? "";
if (!string.IsNullOrWhiteSpace(appConfigEndpoint) || !string.IsNullOrWhiteSpace(appConfigConnectionString))
{
    try
    {
        if (!string.IsNullOrWhiteSpace(appConfigEndpoint))
        {
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(new Uri(appConfigEndpoint), new DefaultAzureCredential())
                     .UseFeatureFlags(featureFlagOptions =>
                     {
                         featureFlagOptions.Label = appConfigLabel;
                         featureFlagOptions.SetRefreshInterval(TimeSpan.FromSeconds(30));
                     })
                     .ConfigureRefresh(refresh =>
                     {
                         refresh.Register("FeatureManagement:Sentinel", refreshAll: true)
                                .SetRefreshInterval(TimeSpan.FromSeconds(30));
                     });
            });
            azureAppConfigRegistered = true;
        }
        else if (!string.IsNullOrWhiteSpace(appConfigConnectionString))
        {
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(appConfigConnectionString)
                     .UseFeatureFlags(featureFlagOptions =>
                     {
                         featureFlagOptions.Label = appConfigLabel;
                         featureFlagOptions.SetRefreshInterval(TimeSpan.FromSeconds(30));
                     })
                     .ConfigureRefresh(refresh =>
                     {
                         refresh.Register("FeatureManagement:Sentinel", refreshAll: true)
                                .SetRefreshInterval(TimeSpan.FromSeconds(30));
                     });
            });
            azureAppConfigRegistered = true;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Failed to configure Azure App Configuration: {ex.Message}");
        Console.WriteLine("Azure App Configuration will be skipped. Feature flags will fallback to local config.");
    }
}

// Add Feature Management
builder.Services.AddFeatureManagement();
// Ensure Azure App Configuration services are registered so the middleware can be used safely
builder.Services.AddAzureAppConfiguration();

// ✅ 12-FACTOR: Externalized Application Insights configuration
var appInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS__CONNECTIONSTRING")
    ?? builder.Configuration["ApplicationInsights:ConnectionString"];

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = appInsightsConnectionString;
});

// Configure sampling percentage
var samplingPercentage = builder.Configuration.GetValue<double?>("ApplicationInsights:SamplingPercentage") ?? 100.0;
builder.Services.Configure<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>(config =>
{
    var samplingProcessorBuilder = config.DefaultTelemetrySink.TelemetryProcessorChainBuilder;

    // Use fixed-rate sampling based on the configured percentage
    samplingProcessorBuilder.UseSampling(samplingPercentage);

    samplingProcessorBuilder.Build();
});

// Register custom telemetry initializers
builder.Services.AddSingleton<ITelemetryInitializer>(new CustomTelemetryInitializer("Demo1"));

var app = builder.Build();

// Flag set when Azure App Configuration provider is successfully added

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSecurityHeaders();
app.UseStatusCodePagesWithReExecute("/Home/Error{0}");
app.UseRouting();

// 🔥 ANTI-PATTERN: Session middleware - kept for demo pages only
app.UseSession();

// Apply Azure App Configuration middleware so feature flags and config refresh are available per-request
if (azureAppConfigRegistered)
{
    app.UseAzureAppConfiguration();
}

app.UseAuthorization();

app.MapStaticAssets();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

/// <summary>
/// Marker partial class used to host the application in integration tests.
/// </summary>
public partial class Program
{
}
