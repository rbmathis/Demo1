using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Demo1.Models;
using Microsoft.FeatureManagement.Mvc;
using Demo1.Features;

namespace Demo1.Controllers;

/// <summary>
/// Provides MVC actions for the home pages (Index, Privacy, Error).
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="logger">The logger to record diagnostic information.</param>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Displays the home page.
    /// </summary>
    /// <returns>The default Index view.</returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Displays the privacy policy page.
    /// </summary>
    /// <returns>The Privacy view.</returns>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Displays the About Us page with company information.
    /// </summary>
    /// <returns>The AboutUs view.</returns>
    public IActionResult AboutUs()
    {
        return View();
    }

    /// <summary>
    /// Displays Feature1 demo page (gated by feature flag).
    /// </summary>
    /// <returns>The Feature1 view if enabled, otherwise NotFound.</returns>
    [FeatureGate(FeatureFlags.Feature1)]
    public IActionResult Feature1()
    {
        _logger.LogInformation("Feature1 accessed");
        return View();
    }

    /// <summary>
    /// Displays the Contact page (gated by feature flag).
    /// </summary>
    /// <returns>The Contact view if enabled, otherwise NotFound.</returns>
    [FeatureGate(FeatureFlags.ContactForm)]
    public IActionResult Contact()
    {
        _logger.LogInformation("Contact page accessed");
        return View();
    }

    /// <summary>
    /// Displays a custom 404 (Not Found) error page.
    /// </summary>
    /// <returns>The Error404 view with <see cref="ErrorViewModel"/>.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error404()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Displays a custom 500 (Internal Server Error) error page.
    /// </summary>
    /// <returns>The Error500 view with <see cref="ErrorViewModel"/>.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error500()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Displays the error page with the current request identifier, if available.
    /// </summary>
    /// <returns>The Error view with <see cref="ErrorViewModel"/>.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
