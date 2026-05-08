namespace Autopilot.Pipeline;

internal interface IStageRunner
{
    Task<StageResult> RunAsync(StageDefinition stage, string prompt, TimeSpan timeout, CancellationToken cancellationToken = default);
}
