using Demo1.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// Configure sampling percentage
var samplingPercentage = builder.Configuration.GetValue<double?>("ApplicationInsights:SamplingPercentage") ?? 100.0;
builder.Services.Configure<TelemetryConfiguration>(config =>
{
    var samplingProcessorBuilder = config.DefaultTelemetrySink.TelemetryProcessorChainBuilder;
    
    // Use fixed-rate sampling based on the configured percentage
    samplingProcessorBuilder.UseSampling(samplingPercentage);
    
    samplingProcessorBuilder.Build();
});

// Register custom telemetry initializers
builder.Services.AddSingleton<ITelemetryInitializer>(new CustomTelemetryInitializer("Demo1"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
