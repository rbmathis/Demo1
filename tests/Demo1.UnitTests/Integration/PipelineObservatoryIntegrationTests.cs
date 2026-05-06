using Demo1;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Integration;

public class PipelineObservatoryIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PipelineObservatoryIntegrationTests(WebApplicationFactory<Program> baseFactory)
    {
        var pipelineServiceMock = new Mock<IGitHubPipelineService>();
        pipelineServiceMock
            .Setup(service => service.GetDashboardAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PipelineObservatoryViewModel());

        var predictionServiceMock = new Mock<IPredictionService>();
        predictionServiceMock
            .Setup(service => service.PredictStage(It.IsAny<string>()))
            .Returns(new PredictionResult(PipelineStage.Triage, 0.2, "test"));

        _factory = baseFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(pipelineServiceMock.Object);
                services.AddSingleton(predictionServiceMock.Object);
            });
        });
    }

    [Fact]
    public async Task Get_Dashboard_ReturnsSuccess()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/pipeline-observatory/dashboard");

        Assert.True(response.IsSuccessStatusCode);
    }
}
