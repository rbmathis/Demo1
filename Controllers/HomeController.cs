using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Demo1.Models;
using Demo1.Services;
using Microsoft.FeatureManagement.Mvc;
using Demo1.Features;
using System.Text;
using System.Runtime.InteropServices;

namespace Demo1.Controllers;

/// <summary>
/// Provides MVC actions for the home pages (Index, Privacy, Error).
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ISearchService _searchService;
    private readonly IWeatherService _weatherService;
    private readonly IUserProfileService _userProfileService;
    private readonly IStyleGeneratorService _styleGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="logger">The logger to record diagnostic information.</param>
    /// <param name="searchService">Search functionality service.</param>
    /// <param name="weatherService">Weather data service.</param>
    /// <param name="userProfileService">User profile management service.</param>
    /// <param name="styleGenerator">Style generation service for demos.</param>
    public HomeController(
        ILogger<HomeController> logger,
        ISearchService searchService,
        IWeatherService weatherService,
        IUserProfileService userProfileService,
        IStyleGeneratorService styleGenerator)
    {
        _logger = logger;
        _searchService = searchService;
        _weatherService = weatherService;
        _userProfileService = userProfileService;
        _styleGenerator = styleGenerator;
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


    // ╔══════════════════════════════════════════════════════════════════════════════╗
    // ║  🎪 ANTI-PATTERN CIRCUS: THE FIVE HORSEMEN OF THE CODEPOCALYPSE 🎪          ║
    // ║  Each page below demonstrates what NOT to do. Now using proper DI!          ║
    // ╚══════════════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// Profile management demo - now using proper DI and encapsulation.
    /// The view still shows "anti-pattern" styling for demo purposes.
    /// </summary>
    public async Task<IActionResult> GodObjectProfile(string action = "", string field = "", string value = "")
    {
        var profile = await _userProfileService.GetProfileAsync("");

        if (!string.IsNullOrEmpty(action) && action == "update" && !string.IsNullOrEmpty(field))
        {
            try
            {
                profile = await _userProfileService.UpdateFieldAsync("", field, value);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid field update attempt: {Field}", field);
                ViewBag.Error = ex.Message;
            }
        }

        ViewBag.Profile = profile;
        ViewBag.Stats = _userProfileService.GetStats();

        return View();
    }

    /// <summary>
    /// Search demo - now using proper service abstraction with parameterized queries.
    /// The view still shows "anti-pattern" styling for demo purposes.
    /// </summary>
    public async Task<IActionResult> RawSqlSearch(string q = "", string table = "users", string orderBy = "id", string customWhere = "")
    {
        var query = new SearchQuery
        {
            term = q,
            table = table,
            orderBy = orderBy,
            customWhere = customWhere,
            unsafeMode = false // No longer unsafe!
        };

        var results = await _searchService.SearchAsync(query);

        ViewBag.Results = results;
        ViewBag.Query = query;
        ViewBag.QueryHistory = _searchService.GetRecentQueries(10);
        ViewBag.TotalQueries = _searchService.TotalQueryCount;

        return View();
    }

    /// <summary>
    /// Calculator demo - demonstrates proper separation of concerns.
    /// The view still displays with "chaos" styling for demo purposes.
    /// </summary>
    public IActionResult ViewLogicCalculator()
    {
        // Data prepared in controller, not computed in view
        var random = new Random();
        var data = new ViewLogicData
        {
            number1 = random.NextDouble() * 100,
            number2 = random.NextDouble() * 100,
            number3 = random.NextDouble() * 100,
            number4 = random.NextDouble() * 100,
            number5 = random.NextDouble() * 100,
            dateString = DateTime.Now.ToString("O"),
            jsonString = "{\"key\": \"value\", \"nested\": {\"a\": 1, \"b\": [1,2,3]}}",
            csvString = "name,age,email\nJohn,25,john@example.com\nJane,30,jane@example.com",
            xmlString = "<root><item id='1'>Hello</item><item id='2'>World</item></root>",
            rawData = new List<object> { 1, "two", 3.0, true },
            debugMode = false,
        };

        ViewBag.Data = data;
        ViewBag.RawQueryString = Request.QueryString.Value;

        return View();
    }

    /// <summary>
    /// Weather demo - now using proper async patterns and DI.
    /// The view still displays with "chaos" styling for demo purposes.
    /// </summary>
    public async Task<IActionResult> CallbackHellWeather(string city = "Chaosville")
    {
        WeatherData? weather = null;
        var errors = new List<string>();

        try
        {
            weather = await _weatherService.GetWeatherAsync(city);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch weather for {City}", city);
            errors.Add($"Weather service error: {ex.Message}");
        }

        weather ??= new WeatherData
        {
            city = city,
            temp = new Random().Next(-10, 35),
            condition = "Unknown",
            isReal = false,
        };

        var stats = _weatherService.GetStats();

        ViewBag.Weather = weather;
        ViewBag.Errors = errors;
        ViewBag.ApiCalls = stats.ApiCallCount;
        ViewBag.CacheStatus = stats.IsHealthy ? "✅ Healthy" : "❌ Unhealthy";

        return View();
    }

    /// <summary>
    /// CSS styling demo - uses service for style generation.
    /// </summary>
    public IActionResult InlineCssHell(int chaos = 5)
    {
        chaos = Math.Clamp(chaos, 1, 11);

        var model = new InlineCssModel
        {
            title = "Welcome to CSS Hell 🔥",
            content = "Where style attributes go to party and best practices go to die",
            fontFamily = _styleGenerator.GetRandomFont(),
            backgroundColor = _styleGenerator.GetRandomColor(),
            textColor = _styleGenerator.GetRandomColor(),
            fontSize = new Random().Next(12, 48),
            blinkSpeed = new Random().Next(100, 1000),
            enableChaos = chaos > 5,
            marqueeText = "🎉 THIS TEXT IS SCROLLING LIKE IT'S 1999 🎉",
            rotationDegrees = new Random().Next(-15, 15),
            items = new List<InlineCssItem>()
        };

        var itemTexts = new[]
        {
            "I have inline styles and I'm not afraid to use them",
            "!important is my middle name",
            "CSS frameworks are for the weak",
            "Who needs a stylesheet when you have ATTITUDE",
            "Semantic HTML? Never heard of her",
            "This div is nested 7 layers deep",
            "Accessibility is just a suggestion",
        };

        for (int i = 0; i < itemTexts.Length; i++)
        {
            model.items.Add(new InlineCssItem
            {
                text = itemTexts[i],
                style = _styleGenerator.GenerateChaosStyle(),
                onclick = $"alert('You clicked item {i + 1}!');",
                isImportant = true
            });
        }

        ViewBag.Model = model;
        ViewBag.ChaosLevel = chaos;
        ViewBag.RandomStyles = Enumerable.Range(0, 20).Select(_ => _styleGenerator.GenerateChaosStyle()).ToList();

        return View();
    }
}
