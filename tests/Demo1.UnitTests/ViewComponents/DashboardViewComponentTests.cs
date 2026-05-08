using Demo1.Models;
using Demo1.Services;
using Demo1.UnitTests.Infrastructure;
using Demo1.ViewComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.ViewComponents;

public class DashboardViewComponentTests
{
    [Fact]
    public async Task Invoke_WithUptimeData_ReturnsDashboardCards()
    {
        var uptime = TimeSpan.FromHours(2) + TimeSpan.FromMinutes(30);
        var requestDurationHistory = new[]
        {
            new PerformanceMetric { MetricName = "RequestDuration", Value = 120, Timestamp = DateTime.UtcNow.AddMinutes(-2) },
            new PerformanceMetric { MetricName = "RequestDuration", Value = 240, Timestamp = DateTime.UtcNow.AddMinutes(-1) },
        };
        var requestCountHistory = new[]
        {
            new PerformanceMetric { MetricName = "RequestCount", Value = 8, Timestamp = DateTime.UtcNow.AddMinutes(-2) },
            new PerformanceMetric { MetricName = "RequestCount", Value = 16, Timestamp = DateTime.UtcNow.AddMinutes(-1) },
        };

        var (component, _, _, _, _) = CreateComponent(
            uptime,
            requestDurationHistory,
            requestCountHistory);

        var result = await component.InvokeAsync();

        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var cards = Assert.IsAssignableFrom<IReadOnlyList<DashboardCardViewModel>>(viewResult.ViewData!.Model);
        Assert.Equal(4, cards.Count);

        Assert.Equal("Uptime", cards[0].Title);
        Assert.Equal("2h 30m", cards[0].Value);
        Assert.Equal("Request Duration", cards[1].Title);
        Assert.Equal("240", cards[1].Value);
        Assert.Equal("Request Count", cards[2].Title);
        Assert.Equal("16", cards[2].Value);
        Assert.Equal("Health Status", cards[3].Title);
        Assert.Equal("Healthy", cards[3].Value);
    }

    [Fact]
    public async Task Invoke_WithEmptyMetrics_ReturnsCardsWithFallbackValues()
    {
        var (component, _, _, _, _) = CreateComponent(
            TimeSpan.FromMinutes(5),
            [],
            []);

        var result = await component.InvokeAsync();

        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var cards = Assert.IsAssignableFrom<IReadOnlyList<DashboardCardViewModel>>(viewResult.ViewData!.Model);
        Assert.Equal(4, cards.Count);

        Assert.Equal("N/A", cards.Single(card => card.Title == "Request Duration").Value);
        Assert.Equal("N/A", cards.Single(card => card.Title == "Request Count").Value);
    }

    [Fact]
    public async Task Invoke_NormalizesSparklinePoints_ToZeroOneRange()
    {
        var requestDurationHistory = new[]
        {
            new PerformanceMetric { MetricName = "RequestDuration", Value = 250, Timestamp = DateTime.UtcNow.AddMinutes(-3) },
            new PerformanceMetric { MetricName = "RequestDuration", Value = 500, Timestamp = DateTime.UtcNow.AddMinutes(-2) },
            new PerformanceMetric { MetricName = "RequestDuration", Value = 375, Timestamp = DateTime.UtcNow.AddMinutes(-1) },
        };
        var requestCountHistory = new[]
        {
            new PerformanceMetric { MetricName = "RequestCount", Value = 2, Timestamp = DateTime.UtcNow.AddMinutes(-3) },
            new PerformanceMetric { MetricName = "RequestCount", Value = 6, Timestamp = DateTime.UtcNow.AddMinutes(-2) },
            new PerformanceMetric { MetricName = "RequestCount", Value = 4, Timestamp = DateTime.UtcNow.AddMinutes(-1) },
        };
        var (component, _, _, _, _) = CreateComponent(
            TimeSpan.FromMinutes(10),
            requestDurationHistory,
            requestCountHistory);

        var result = await component.InvokeAsync();

        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        var cards = Assert.IsAssignableFrom<IReadOnlyList<DashboardCardViewModel>>(viewResult.ViewData!.Model);
        var requestDurationCard = cards.Single(card => card.Title == "Request Duration");
        var requestCountCard = cards.Single(card => card.Title == "Request Count");

        Assert.All(requestDurationCard.SparklinePoints, point => Assert.InRange(point, 0d, 1d));
        Assert.All(requestCountCard.SparklinePoints, point => Assert.InRange(point, 0d, 1d));
    }

    [Fact]
    public async Task Invoke_LogsInformationMessage()
    {
        var (component, _, _, _, loggerMock) = CreateComponent(
            TimeSpan.FromMinutes(1),
            [],
            []);

        _ = await component.InvokeAsync();

        loggerMock.VerifyLog(LogLevel.Information, Times.Once());
    }

    private static (
        DashboardViewComponent component,
        Mock<IUptimeService> uptimeServiceMock,
        Mock<IPerformanceMetricsService> metricsServiceMock,
        Mock<IFeatureManagerSnapshot> featureManagerMock,
        Mock<ILogger<DashboardViewComponent>> loggerMock) CreateComponent(
        TimeSpan uptime,
        IReadOnlyList<PerformanceMetric> requestDurationHistory,
        IReadOnlyList<PerformanceMetric> requestCountHistory)
    {
        var uptimeServiceMock = new Mock<IUptimeService>();
        uptimeServiceMock.Setup(service => service.GetUptime()).Returns(uptime);

        var metricsServiceMock = new Mock<IPerformanceMetricsService>();
        metricsServiceMock
            .Setup(service => service.GetHistory("RequestDuration", 60))
            .Returns(requestDurationHistory);
        metricsServiceMock
            .Setup(service => service.GetHistory("RequestCount", 60))
            .Returns(requestCountHistory);

        var featureManagerMock = new Mock<IFeatureManagerSnapshot>();
        featureManagerMock
            .Setup(manager => manager.IsEnabledAsync(Demo1.Features.FeatureFlags.DashboardHomePage))
            .ReturnsAsync(true);

        var loggerMock = new Mock<ILogger<DashboardViewComponent>>();

        var component = new DashboardViewComponent(
            uptimeServiceMock.Object,
            metricsServiceMock.Object,
            featureManagerMock.Object,
            loggerMock.Object);

        return (component, uptimeServiceMock, metricsServiceMock, featureManagerMock, loggerMock);
    }
}
