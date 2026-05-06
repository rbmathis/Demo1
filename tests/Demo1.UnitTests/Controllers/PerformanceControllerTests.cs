using Demo1.Controllers;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Controllers;

public class PerformanceControllerTests
{
    private readonly Mock<IPerformanceMetricsService> _metricsServiceMock;
    private readonly Mock<ILogger<PerformanceController>> _loggerMock;
    private readonly PerformanceController _controller;

    public PerformanceControllerTests()
    {
        _metricsServiceMock = new Mock<IPerformanceMetricsService>();
        _loggerMock = new Mock<ILogger<PerformanceController>>();
        _controller = new PerformanceController(_metricsServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Dashboard_Returns_ViewResult_With_Budgets()
    {
        var budgets = new List<PerformanceBudget>
        {
            new() { MetricName = "LCP", WarningThreshold = 2500, ErrorThreshold = 4000, Unit = "ms" },
            new() { MetricName = "CLS", WarningThreshold = 0.1, ErrorThreshold = 0.25, Unit = "" }
        };
        _metricsServiceMock.Setup(s => s.GetBudgets()).Returns(budgets);

        var result = _controller.Dashboard();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<PerformanceBudget>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public void Report_ValidMetric_Returns_Ok()
    {
        var metric = new PerformanceMetric
        {
            MetricName = "LCP",
            Value = 1500,
            Unit = "ms",
            PageUrl = "/home"
        };

        var result = _controller.Report(metric);

        Assert.IsType<OkResult>(result);
        _metricsServiceMock.Verify(s => s.ReportMetric(metric), Times.Once);
    }

    [Fact]
    public void Report_InvalidModelState_Returns_BadRequest()
    {
        _controller.ModelState.AddModelError("MetricName", "Required");
        var metric = new PerformanceMetric();

        var result = _controller.Report(metric);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Report_ServiceException_Returns_StatusCode500()
    {
        var metric = new PerformanceMetric { MetricName = "LCP", Value = 1000, PageUrl = "/test" };
        _metricsServiceMock.Setup(s => s.ReportMetric(It.IsAny<PerformanceMetric>()))
            .Throws(new InvalidOperationException("Service error"));

        var result = _controller.Report(metric);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public void History_ValidMetricName_Returns_JsonResult()
    {
        var history = new List<PerformanceMetric>
        {
            new() { MetricName = "LCP", Value = 1200, Timestamp = DateTime.UtcNow }
        };
        _metricsServiceMock.Setup(s => s.GetHistory("LCP", 60)).Returns(history);

        var result = _controller.History("LCP", 60);

        Assert.IsType<JsonResult>(result);
    }

    [Fact]
    public void History_EmptyMetricName_Returns_BadRequest()
    {
        var result = _controller.History("", 60);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void History_NullMetricName_Returns_BadRequest()
    {
        var result = _controller.History(null!, 60);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
