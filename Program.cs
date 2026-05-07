using Asp.Versioning;
using Demo1.Middleware;
using Demo1.Models;
using Demo1.Telemetry;
using Demo1.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.FeatureManagement;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Demo1 API",
        Version = "v1"
    });
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Demo1 API",
        Version = "v2"
    });
});

// ✅ Rate Limiting: Configure IP-based rate limiting using built-in middleware
builder.Services.Configure<RateLimitingOptions>(
    builder.Configuration.GetSection(RateLimitingOptions.SectionName));

var rateLimitOptions = builder.Configuration
    .GetSection(RateLimitingOptions.SectionName)
    .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitOptions.PermitLimit,
                Window = TimeSpan.FromSeconds(rateLimitOptions.WindowInSeconds),
                QueueLimit = rateLimitOptions.QueueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.Headers["Retry-After"] =
            rateLimitOptions.WindowInSeconds.ToString();
        context.HttpContext.Response.Headers["X-RateLimit-Limit"] =
            rateLimitOptions.PermitLimit.ToString();
        context.HttpContext.Response.Headers["X-RateLimit-Remaining"] = "0";
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.", cancellationToken);
    };
});

// ✅ 12-FACTOR: Register application services via dependency injection
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISecurityLabService, SecurityLabService>();
builder.Services.AddSingleton<ISearchService, InMemorySearchService>();
builder.Services.AddSingleton<IWeatherService, MockWeatherService>();
builder.Services.AddSingleton<IUserProfileService, InMemoryUserProfileService>();
builder.Services.AddSingleton<IStyleGeneratorService, StyleGeneratorService>();
builder.Services.AddSingleton<IUptimeService, UptimeService>();
builder.Services.AddSingleton<IPerformanceMetricsService, PerformanceMetricsService>();

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
var enableSwagger = app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwagger");

// Flag set when Azure App Configuration provider is successfully added

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo1 API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Demo1 API v2");
    });
}

app.UseHttpsRedirection();
app.UseMiddleware<ServerTimingMiddleware>();
app.UseSecurityHeaders();
app.UseSecurityLabHeaders();
app.UseStatusCodePagesWithReExecute("/Home/Error{0}");
app.UseRouting();
app.UseRateLimiter();
app.UseRateLimitHeaders();

// 🔥 ANTI-PATTERN: Session middleware - kept for demo pages only
app.UseSession();

// Apply Azure App Configuration middleware so feature flags and config refresh are available per-request
if (azureAppConfigRegistered)
{
    app.UseAzureAppConfiguration();
}

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
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
