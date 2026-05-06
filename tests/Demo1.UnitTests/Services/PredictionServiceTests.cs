using Demo1.Models;
using Demo1.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Services;

public class PredictionServiceTests
{
    private static PredictionService CreateService()
    {
        return new PredictionService(Mock.Of<ILogger<PredictionService>>());
    }

    [Fact]
    public void PredictStage_ImplementKeywords_PredictsImplementStage()
    {
        var service = CreateService();

        var result = service.PredictStage("Please implement code, build, and fix this issue.");

        Assert.Equal(PipelineStage.Implement, result.PredictedStage);
        Assert.Equal(0.8, result.Confidence);
    }

    [Fact]
    public void PredictStage_EmptyText_ReturnsTriageWithZeroConfidence()
    {
        var service = CreateService();

        var result = service.PredictStage("   ");

        Assert.Equal(PipelineStage.Triage, result.PredictedStage);
        Assert.Equal(0, result.Confidence);
    }

    [Fact]
    public void PredictStage_AmbiguousText_IsDeterministic()
    {
        var service = CreateService();
        const string text = "Need a plan for this bug";

        var first = service.PredictStage(text);
        var second = service.PredictStage(text);

        Assert.Equal(first, second);
        Assert.Equal(PipelineStage.Triage, first.PredictedStage);
        Assert.Equal(0.2, first.Confidence);
    }
}
