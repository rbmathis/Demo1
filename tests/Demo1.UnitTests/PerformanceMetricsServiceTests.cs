using Demo1.Models;
using Demo1.Services;
using Microsoft.Extensions.Configuration;

namespace Demo1.UnitTests;

public class PerformanceMetricsServiceTests
{
    private static PerformanceMetricsService CreateService(IConfiguration? config = null)
    {
        if (config == null)
        {
            var configData = new Dictionary<string, string?>
            {
                ["PerformanceBudgets:0:MetricName"] = "LCP",
                ["PerformanceBudgets:0:WarningThreshold"] = "2500",
                ["PerformanceBudgets:0:ErrorThreshold"] = "4000",
                ["PerformanceBudgets:0:Unit"] = "ms",
                ["PerformanceBudgets:1:MetricName"] = "TTFB",
                ["PerformanceBudgets:1:WarningThreshold"] = "800",
                ["PerformanceBudgets:1:ErrorThreshold"] = "1800",
                ["PerformanceBudgets:1:Unit"] = "ms",
                ["PerformanceBudgets:2:MetricName"] = "CLS",
                ["PerformanceBudgets:2:WarningThreshold"] = "0.1",
                ["PerformanceBudgets:2:ErrorThreshold"] = "0.25",
                ["PerformanceBudgets:2:Unit"] = "",
                ["PerformanceBudgets:3:MetricName"] = "FID",
                ["PerformanceBudgets:3:WarningThreshold"] = "100",
                ["PerformanceBudgets:3:ErrorThreshold"] = "300",
                ["PerformanceBudgets:3:Unit"] = "ms"
            };

            config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        return new PerformanceMetricsService(config);
    }

    [Fact]
    public void ReportMetric_StoresData()
    {
        var service = CreateService();
        var metric = new PerformanceMetric
        {
            MetricName = "LCP",
            Value = 1500,
            Unit = "ms",
            PageUrl = "/home"
        };

        service.ReportMetric(metric);

        var history = service.GetHistory("LCP", 60);
        Assert.Single(history);
        Assert.Equal(1500, history.First().Value);
    }

    [Fact]
    public void GetHistory_FiltersByMetricName()
    {
        var service = CreateService();

        service.ReportMetric(new PerformanceMetric { MetricName = "LCP", Value = 1500, PageUrl = "/" });
        service.ReportMetric(new PerformanceMetric { MetricName = "CLS", Value = 0.05, PageUrl = "/" });
        service.ReportMetric(new PerformanceMetric { MetricName = "LCP", Value = 2000, PageUrl = "/" });

        var lcpHistory = service.GetHistory("LCP", 60);
        var clsHistory = service.GetHistory("CLS", 60);

        Assert.Equal(2, lcpHistory.Count());
        Assert.Single(clsHistory);
    }

    [Fact]
    public void GetHistory_FiltersByTimeWindow()
    {
        var service = CreateService();

        // Add a metric with a timestamp in the past beyond the window
        var oldMetric = new PerformanceMetric
        {
            MetricName = "LCP",
            Value = 3000,
            PageUrl = "/",
            Timestamp = DateTime.UtcNow.AddMinutes(-120)
        };
        service.ReportMetric(oldMetric);

        // Add a recent metric
        var recentMetric = new PerformanceMetric
        {
            MetricName = "LCP",
            Value = 1500,
            PageUrl = "/",
            Timestamp = DateTime.UtcNow
        };
        service.ReportMetric(recentMetric);

        var history = service.GetHistory("LCP", 60);

        Assert.Single(history);
        Assert.Equal(1500, history.First().Value);
    }

    [Fact]
    public void GetHistory_IsCaseInsensitive()
    {
        var service = CreateService();

        service.ReportMetric(new PerformanceMetric { MetricName = "LCP", Value = 1500, PageUrl = "/" });

        var history = service.GetHistory("lcp", 60);

        Assert.Single(history);
    }

    [Fact]
    public void GetBudgets_ReturnsConfiguredBudgets()
    {
        var service = CreateService();

        var budgets = service.GetBudgets().ToList();

        Assert.Equal(4, budgets.Count);
        Assert.Contains(budgets, b => b.MetricName == "LCP" && b.WarningThreshold == 2500 && b.ErrorThreshold == 4000);
        Assert.Contains(budgets, b => b.MetricName == "TTFB" && b.WarningThreshold == 800);
        Assert.Contains(budgets, b => b.MetricName == "CLS" && b.WarningThreshold == 0.1);
        Assert.Contains(budgets, b => b.MetricName == "FID" && b.ErrorThreshold == 300);
    }

    [Fact]
    public void ReportMetric_CapsAtMaxEntries()
    {
        var service = CreateService();

        // Add 1050 entries to exceed the 1000 cap
        for (int i = 0; i < 1050; i++)
        {
            service.ReportMetric(new PerformanceMetric
            {
                MetricName = "LCP",
                Value = i,
                PageUrl = "/",
                Timestamp = DateTime.UtcNow
            });
        }

        // GetHistory returns all stored entries for "LCP" within time window
        var history = service.GetHistory("LCP", 60);

        // Should be capped at 1000
        Assert.True(history.Count() <= 1000);
        // Oldest entries should have been evicted (values 0-49 gone)
        Assert.DoesNotContain(history, m => m.Value < 50);
    }
}
