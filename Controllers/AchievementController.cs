using System.Diagnostics;
using Demo1.Data;
using Demo1.Models;
using Demo1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Controllers;

/// <summary>
/// Controller for the Achievement system — trophy case, badges API, and anti-pattern demo.
/// </summary>
[Route("[controller]")]
public class AchievementController : Controller
{
    private readonly IAchievementService _achievementService;
    private readonly ILogger<AchievementController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementController"/> class.
    /// </summary>
    /// <param name="achievementService">The achievement service for querying badge data.</param>
    /// <param name="logger">The logger instance.</param>
    public AchievementController(IAchievementService achievementService, ILogger<AchievementController> logger)
    {
        _achievementService = achievementService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the trophy case page showing all achievements with progress indicators.
    /// </summary>
    /// <returns>The trophy case view with achievement progress data.</returns>
    [HttpGet("TrophyCase")]
    public async Task<IActionResult> TrophyCase()
    {
        var sessionId = HttpContext.Session.Id;
        var model = await _achievementService.GetTrophyCaseAsync(sessionId);
        return View(model);
    }

    /// <summary>
    /// 🔥 ANTI-PATTERN: Demonstrates synchronous inline achievement processing.
    /// This blocks the request thread with synchronous database operations —
    /// exactly what the Channel + BackgroundService pattern avoids.
    /// </summary>
    /// <returns>The anti-pattern view with timing comparison data.</returns>
    [HttpGet("SynchronousAntiPattern")]
    public IActionResult SynchronousAntiPattern([FromServices] AchievementDbContext db)
    {
        var stopwatch = Stopwatch.StartNew();
        var sessionId = HttpContext.Session.Id;

        // 🔥 ANTI-PATTERN: Synchronous database queries blocking the request thread
        var achievements = db.Achievements.ToList();
        var events = db.AchievementEvents
            .Where(e => e.SessionId == sessionId)
            .ToList();
        var earned = db.UserAchievements
            .Where(ua => ua.SessionId == sessionId)
            .ToList();

        // 🔥 ANTI-PATTERN: Inline rule evaluation on every request
        var progressList = new List<AchievementProgress>();
        var rulesEvaluated = 0;

        foreach (var achievement in achievements)
        {
            rulesEvaluated++;
            var earnedRecord = earned.FirstOrDefault(e => e.AchievementId == achievement.Id);
            var currentProgress = AchievementService.CalculateProgress(achievement, events);

            progressList.Add(new AchievementProgress
            {
                Achievement = achievement,
                IsEarned = earnedRecord != null,
                EarnedAt = earnedRecord?.EarnedAt,
                CurrentProgress = currentProgress,
                Target = achievement.Target
            });
        }

        stopwatch.Stop();

        var model = new SynchronousAntiPatternViewModel
        {
            Achievements = progressList,
            ElapsedMs = stopwatch.ElapsedMilliseconds,
            EventsQueried = events.Count,
            RulesEvaluated = rulesEvaluated
        };

        _logger.LogWarning(
            "Synchronous achievement check completed in {ElapsedMs}ms — this blocks the thread!",
            model.ElapsedMs);

        return View(model);
    }

    /// <summary>
    /// JSON API endpoint returning earned achievements for the current session.
    /// </summary>
    /// <returns>A JSON array of earned achievement data.</returns>
    [HttpGet("api/badges")]
    public async Task<IActionResult> GetBadges()
    {
        var sessionId = HttpContext.Session.Id;
        var achievements = await _achievementService.GetEarnedAchievementsAsync(sessionId);
        return Json(achievements.Select(a => new
        {
            a.Achievement.Name,
            a.Achievement.Icon,
            a.Achievement.Description,
            a.EarnedAt
        }));
    }
}
