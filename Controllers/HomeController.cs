using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Demo1.Models;

namespace Demo1.Controllers;

/// <summary>
/// Provides MVC actions for the home pages (Index, Privacy, AboutUs, Error).
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
    /// Displays the about us page.
    /// </summary>
    /// <returns>The AboutUs view.</returns>
    public IActionResult AboutUs()
    {
        return View();
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
