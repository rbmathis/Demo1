using Microsoft.AspNetCore.Mvc;
using Demo1.Services;

namespace Demo1.Controllers;

/// <summary>
/// Provides MVC actions for the profile management demo pages (GodObjectProfile,
/// GodObjectProfileUpdate). Routes are preserved at <c>/Home/[action]</c> via the
/// class-level route attribute so all existing URLs remain valid after the refactor.
/// </summary>
[Route("Home/[action]")]
public class ProfileController : Controller
{
    private readonly ILogger<ProfileController> _logger;
    private readonly IUserProfileService _userProfileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileController"/> class.
    /// </summary>
    /// <param name="logger">The logger to record diagnostic information.</param>
    /// <param name="userProfileService">User profile management service.</param>
    public ProfileController(
        ILogger<ProfileController> logger,
        IUserProfileService userProfileService)
    {
        _logger = logger;
        _userProfileService = userProfileService;
    }

    /// <summary>
    /// Displays the profile management demo with the current profile and profile statistics.
    /// </summary>
    /// <remarks>
    /// The legacy GET query parameters remain in the action signature so older links, bookmarks, and
    /// query-string-based requests continue to resolve safely after the CSRF fix moved profile updates
    /// to the POST-only <see cref="GodObjectProfileUpdate(string, string)"/> action. These values are
    /// intentionally ignored.
    /// </remarks>
    /// <param name="action">Unused legacy query parameter retained so older GET query strings can be safely ignored.</param>
    /// <param name="field">Unused legacy query parameter retained so older GET query strings can be safely ignored.</param>
    /// <param name="value">Unused legacy query parameter retained so older GET query strings can be safely ignored.</param>
    /// <returns>The GodObjectProfile view.</returns>
    public async Task<IActionResult> GodObjectProfile(string action = "", string field = "", string value = "")
    {
        var profile = await _userProfileService.GetProfileAsync("");

        ViewBag.Profile = profile;
        ViewBag.Stats = _userProfileService.GetStats();

        return View();
    }

    /// <summary>
    /// Updates a single profile field and redirects back to the profile page.
    /// </summary>
    /// <param name="field">The profile field to update.</param>
    /// <param name="value">The new value for the field.</param>
    /// <returns>A redirect to <see cref="GodObjectProfile(string, string, string)"/>.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GodObjectProfileUpdate(string field, string value)
    {
        try
        {
            await _userProfileService.UpdateFieldAsync("", field, value);
            TempData["Success"] = "Profile updated successfully.";
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid profile field update attempt.");
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(GodObjectProfile));
    }
}
