using Demo1.Middleware;
using Demo1.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.FeatureManagement;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();

// 🔥 ANTI-PATTERN: Add session for maximum state abuse
// Sessions stored in memory because distributed caching is for the weak
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".Demo1.ChaosCookie"; // naming is hard
});

// Flag set when Azure App Configuration provider is successfully added
var azureAppConfigRegistered = false;

// Register Azure App Configuration (Managed Identity) using the provided endpoint or connection string
var appConfigEndpoint = builder.Configuration["AzureAppConfiguration:Endpoint"] ?? "https://rbmappconfig.azconfig.io";
var appConfigConnectionString = builder.Configuration["AzureAppConfiguration:ConnectionString"] ?? Environment.GetEnvironmentVariable("AZURE_APP_CONFIG_CONNECTIONSTRING");
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

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
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

// 🔥 ANTI-PATTERN: Session middleware for global state abuse
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
