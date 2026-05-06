using Demo1.Controllers;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Controllers;

public class PipelineObservatoryControllerTests
{
    private static PipelineObservatoryController CreateController(
        Mock<IGitHubPipelineService>? pipelineServiceMock = null,
        Mock<IPredictionService>? predictionServiceMock = null,
        Mock<ILogger<PipelineObservatoryController>>? loggerMock = null)
    {
        return new PipelineObservatoryController(
            (pipelineServiceMock ?? new Mock<IGitHubPipelineService>()).Object,
            (predictionServiceMock ?? new Mock<IPredictionService>()).Object,
            (loggerMock ?? new Mock<ILogger<PipelineObservatoryController>>()).Object);
    }

    [Fact]
    public async Task Dashboard_ReturnsViewResult_WithServiceModel()
    {
        var expectedModel = new PipelineObservatoryViewModel();
        var pipelineServiceMock = new Mock<IGitHubPipelineService>();
        pipelineServiceMock
            .Setup(service => service.GetDashboardAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedModel);
        var controller = CreateController(pipelineServiceMock: pipelineServiceMock);

        var result = await controller.Dashboard(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(expectedModel, view.Model);
    }

    [Fact]
    public async Task Issue_RunMissing_ReturnsNotFound()
    {
        var pipelineServiceMock = new Mock<IGitHubPipelineService>();
        pipelineServiceMock
            .Setup(service => service.GetRunByIssueNumberAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PipelineRun?)null);
        var controller = CreateController(pipelineServiceMock: pipelineServiceMock);

        var result = await controller.Issue(42, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Issue_RunFound_ReturnsViewResult()
    {
        var run = new PipelineRun { IssueNumber = 42, Title = "Pipeline — Triage" };
        var pipelineServiceMock = new Mock<IGitHubPipelineService>();
        pipelineServiceMock
            .Setup(service => service.GetRunByIssueNumberAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(run);
        var controller = CreateController(pipelineServiceMock: pipelineServiceMock);

        var result = await controller.Issue(42, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(run, view.Model);
    }

    [Fact]
    public void Predict_EmptyInput_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = controller.Predict(new PredictionRequest { IssueText = " " });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Predict_ValidInput_ReturnsJsonResult()
    {
        var prediction = new PredictionResult(PipelineStage.Plan, 0.4, "Matched keywords");
        var predictionServiceMock = new Mock<IPredictionService>();
        predictionServiceMock
            .Setup(service => service.PredictStage("Need a concrete plan for rollout"))
            .Returns(prediction);
        var controller = CreateController(predictionServiceMock: predictionServiceMock);

        var result = controller.Predict(new PredictionRequest { IssueText = "Need a concrete plan for rollout" });

        var json = Assert.IsType<JsonResult>(result);
        Assert.Same(prediction, json.Value);
    }
}
