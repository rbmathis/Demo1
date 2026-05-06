using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demo1.Controllers;

/// <summary>
/// Exposes AI Pipeline Observatory dashboard and prediction endpoints.
/// </summary>
[Route("pipeline-observatory")]
public class PipelineObservatoryController : Controller
{
    private readonly IGitHubPipelineService _gitHubPipelineService;
    private readonly IPredictionService _predictionService;
    private readonly ILogger<PipelineObservatoryController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineObservatoryController"/> class.
    /// </summary>
    /// <param name="gitHubPipelineService">Pipeline run data service.</param>
    /// <param name="predictionService">Prediction service.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public PipelineObservatoryController(
        IGitHubPipelineService gitHubPipelineService,
        IPredictionService predictionService,
        ILogger<PipelineObservatoryController> logger)
    {
        _gitHubPipelineService = gitHubPipelineService;
        _predictionService = predictionService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the pipeline observatory dashboard.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The observatory dashboard view model.</returns>
    [HttpGet("")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var model = await _gitHubPipelineService.GetDashboardAsync(cancellationToken: cancellationToken);
        return View(model);
    }

    /// <summary>
    /// Displays a single run mapped from an issue number.
    /// </summary>
    /// <param name="id">Issue number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issue run view when found; otherwise 404.</returns>
    [HttpGet("issue/{id:int}")]
    public async Task<IActionResult> Issue(int id, CancellationToken cancellationToken)
    {
        var run = await _gitHubPipelineService.GetRunByIssueNumberAsync(id, cancellationToken);
        if (run is null)
        {
            return NotFound();
        }

        return View(run);
    }

    /// <summary>
    /// Predicts a likely pipeline stage from issue text.
    /// </summary>
    /// <param name="request">Prediction request payload.</param>
    /// <returns>A JSON prediction response.</returns>
    [HttpPost("predict")]
    [ValidateAntiForgeryToken]
    public IActionResult Predict([FromForm] PredictionRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.IssueText))
        {
            _logger.LogWarning("Prediction request rejected due to missing issue text.");
            return BadRequest(new { error = "Issue text is required." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var prediction = _predictionService.PredictStage(request.IssueText);
        return Json(prediction);
    }
}
