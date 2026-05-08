using Demo1.Features;
using Demo1.IntegrationTests.Fixtures;
using Demo1.Models;
using System.Net.Http.Json;

namespace Demo1.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for dashboard feature-flag behavior on the home page.
/// </summary>
[Collection("Integration")]
public class DashboardFeatureTests
{
    private readonly Demo1WebApplicationFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardFeatureTests"/> class.
    /// </summary>
    /// <param name="factory">The shared web application factory.</param>
    public DashboardFeatureTests(Demo1WebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies the home page renders the welcome content when the dashboard flag is disabled.
    /// </summary>
    [Fact]
    public async Task Index_WhenDashboardFlagOff_ShowsWelcomePage()
    {
        using var flaggedFactory = _factory.WithFeatureFlags(new Dictionary<string, bool>
        {
            [FeatureFlags.DashboardHomePage] = false
        });

        var client = flaggedFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Welcome");
        content.Should().NotContain("dashboard-grid");
    }

    /// <summary>
    /// Verifies the home page renders dashboard cards when the dashboard flag is enabled.
    /// </summary>
    [Fact]
    public async Task Index_WhenDashboardFlagOn_ShowsDashboardCards()
    {
        using var flaggedFactory = _factory.WithFeatureFlags(new Dictionary<string, bool>
        {
            [FeatureFlags.DashboardHomePage] = true
        });

        var client = flaggedFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await client.PostAsJsonAsync("/Performance/Report", new PerformanceMetric
        {
            MetricName = "RequestDuration",
            Value = 120,
            Unit = "ms",
            PageUrl = "/"
        });
        await client.PostAsJsonAsync("/Performance/Report", new PerformanceMetric
        {
            MetricName = "RequestCount",
            Value = 5,
            Unit = string.Empty,
            PageUrl = "/"
        });

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("dashboard-grid");
        content.Should().Contain("<svg");
    }
}
