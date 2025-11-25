using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Demo1.Models;
using Microsoft.FeatureManagement.Mvc;
using Demo1.Features;
using System.Text;
using System.Runtime.InteropServices;

// TODO: fix this later
// TODO: why does this work?
// TODO: DO NOT DEPLOY TO PRODUCTION
// HACK: temporary fix from 2019
// FIXME: ask Dave about this

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


    // ╔══════════════════════════════════════════════════════════════════════════════╗
    // ║  🎪 ANTI-PATTERN CIRCUS: THE FIVE HORSEMEN OF THE CODEPOCALYPSE 🎪          ║
    // ║  Each page below violates at least 10 best practices. Collect them all!     ║
    // ╚══════════════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// GOD OBJECT PROFILE PAGE
    /// Features: A model with 100+ properties, mutable everything, global state
    /// Anti-patterns: God Object, Primitive Obsession, No Encapsulation
    /// </summary>
    public IActionResult GodObjectProfile(string action = "", string field = "", string value = "")
    {
        // Get or create the global user profile (thread safety is for the weak)
        if (GodObjectHelper.Current == null)
        {
            GodObjectHelper.Current = new GodObjectProfile
            {
                name = "Default User",
                Name2 = "Also Default",
                NAME = "DEFAULT USER",
                n = "D",
                age = 25,
                Age = 26, // they had a birthday on a different server
                email = "user@example.com",
                password = "password123", // stored in plaintext, obviously
                ssn = "123-45-6789",
                creditCard = "4111-1111-1111-1111",
                creditCardCvv = "123",
                hogwartsHouse = "Hufflepuff",
                midichlorianCount = 9001,
            };
            GodObjectHelper.AllUsers.Add(GodObjectHelper.Current);
            GodObjectHelper.Passwords["user@example.com"] = "password123";
        }

        // Handle field updates via query string (security what?)
        if (!string.IsNullOrEmpty(action) && action == "update" && !string.IsNullOrEmpty(field))
        {
            try
            {
                // Use reflection to set any property (WCGW?)
                var prop = typeof(GodObjectProfile).GetProperty(field);
                if (prop != null)
                {
                    var convertedValue = Convert.ChangeType(value, prop.PropertyType);
                    prop.SetValue(GodObjectHelper.Current, convertedValue);
                }
            }
            catch (Exception ex)
            {
                // Swallow the exception and continue
                GlobalDatabase.LastException = ex;
            }
        }

        ViewBag.Profile = GodObjectHelper.Current;
        ViewBag.AllPasswords = GodObjectHelper.Passwords; // expose all passwords to view
        ViewBag.TotalUsers = GodObjectHelper.AllUsers.Count;
        ViewBag.IsValid = GodObjectHelper.Current.IsValid(); // always true lol
        ViewBag.DisplayJson = GodObjectHelper.Current.ToJson(); // includes sensitive data

        return View();
    }

    /// <summary>
    /// RAW SQL SEARCH PAGE
    /// Features: String concatenation SQL, hardcoded connection strings, no parameterization
    /// Anti-patterns: SQL Injection, Hardcoded Credentials, Security by Obscurity
    /// </summary>
    public IActionResult RawSqlSearch(string q = "", string table = "users", string orderBy = "id", string customWhere = "")
    {
        var results = new List<SearchResult>();
        var query = new SearchQuery
        {
            term = q,
            table = table,
            orderBy = orderBy,
            customWhere = customWhere,
            unsafeMode = true // always unsafe
        };

        // Build our "secure" SQL query using string concatenation
        var sql = $"SELECT * FROM {query.table} WHERE 1=1";

        if (!string.IsNullOrEmpty(q))
        {
            // No escaping, no parameterization, just vibes
            sql += $" AND (title LIKE '%{q}%' OR description LIKE '%{q}%')";
        }

        if (!string.IsNullOrEmpty(customWhere))
        {
            // Let users write their own WHERE clause! What could go wrong?
            sql += $" AND ({customWhere})";
        }

        sql += $" ORDER BY {orderBy}";

        // Log the query (including any injection attempts)
        SearchDatabase.QueryHistory.Add($"[{DateTime.Now}] {sql}");
        GlobalDatabase.SqlQueryLog.Add(sql);

        // Since we don't have a real DB, fake it with our static data
        results = SearchDatabase.FakeData;

        if (!string.IsNullOrEmpty(q))
        {
            results = results.Where(r =>
                r.title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                r.description.Contains(q, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        // Add the SQL to each result for "transparency"
        foreach (var result in results)
        {
            result.rawSql = sql;
            result.connectionUsed = SearchDatabase.ConnectionString; // show the connection string!
        }

        ViewBag.Results = results;
        ViewBag.Query = query;
        ViewBag.GeneratedSql = sql;
        ViewBag.ConnectionString = SearchDatabase.ConnectionString;
        ViewBag.QueryHistory = SearchDatabase.QueryHistory.TakeLast(10).ToList();
        ViewBag.TotalQueries = SearchDatabase.QueryHistory.Count;

        return View();
    }

    /// <summary>
    /// VIEW LOGIC CALCULATOR PAGE
    /// Features: All business logic will be in the Razor view
    /// Anti-patterns: Logic in Views, No Separation of Concerns, View doing Controller's job
    /// </summary>
    public IActionResult ViewLogicCalculator()
    {
        // Controller does NOTHING. All logic is in the view.
        // The view will parse query strings, do math, handle errors, everything.

        var data = new ViewLogicData
        {
            number1 = GlobalDatabase.SharedRandom.NextDouble() * 100,
            number2 = GlobalDatabase.SharedRandom.NextDouble() * 100,
            number3 = GlobalDatabase.SharedRandom.NextDouble() * 100,
            number4 = GlobalDatabase.SharedRandom.NextDouble() * 100,
            number5 = GlobalDatabase.SharedRandom.NextDouble() * 100,
            dateString = DateTime.Now.ToString(),
            jsonString = "{\"key\": \"value\", \"nested\": {\"a\": 1, \"b\": [1,2,3]}}",
            csvString = "name,age,email\nJohn,25,john@example.com\nJane,30,jane@example.com",
            xmlString = "<root><item id='1'>Hello</item><item id='2'>World</item></root>",
            rawData = new List<object> { 1, "two", 3.0, true, null, new { x = 1, y = 2 } },
            connectionString = "Server=prod;Password=admin123;", // oops
            apiKey = "sk-1234567890abcdef", // double oops
            debugMode = true,
        };

        // Pass raw data to view, let it figure everything out
        ViewBag.Data = data;
        ViewBag.Request = HttpContext.Request; // give view access to request
        ViewBag.RawQueryString = Request.QueryString.Value;

        return View();
    }

    /// <summary>
    /// CALLBACK HELL WEATHER PAGE
    /// Features: Nested async, artificial delays, race conditions
    /// Anti-patterns: Callback Hell, Fake Async, Exception Swallowing
    /// </summary>
    public async Task<IActionResult> CallbackHellWeather(string city = "Chaosville")
    {
        WeatherData weather = null;
        var errors = new List<string>();

        try
        {
            // Nested async calls because callbacks are fun
            await Task.Run(async () =>
            {
                await Task.Delay(100); // artificial delay for "realism"

                await Task.Run(async () =>
                {
                    await Task.Delay(50);

                    await Task.Run(async () =>
                    {
                        await Task.Delay(25);

                        // Finally, get the weather (it's all fake)
                        weather = await GetFakeWeatherAsync(city);

                        // More nested operations!
                        await Task.Run(async () =>
                        {
                            await Task.Delay(10);
                            WeatherCache.LastUpdated = DateTime.Now;
                            WeatherCache.ApiCallCount++;
                        });
                    });
                });
            });
        }
        catch (Exception ex)
        {
            // Swallow ALL exceptions
            WeatherCache.SwallowedExceptions.Add(ex);
            WeatherCache.LastError = ex.Message;
            errors.Add($"Something went wrong but we'll never know what: {ex.GetType().Name}");
        }

        // If weather is still null, make something up
        weather ??= new WeatherData
        {
            city = city,
            temp = GlobalDatabase.SharedRandom.Next(-50, 120),
            condition = "Unknown (API probably down)",
            isReal = false,
        };

        ViewBag.Weather = weather;
        ViewBag.SwallowedExceptions = WeatherCache.SwallowedExceptions.Count;
        ViewBag.Errors = errors;
        ViewBag.ApiCalls = WeatherCache.ApiCallCount;
        ViewBag.CacheStatus = WeatherCache.IsApiDown ? "💀 Dead" : "🤷 Maybe working?";

        return View();
    }

    private async Task<WeatherData> GetFakeWeatherAsync(string city)
    {
        // Fake API call with artificial delay
        await Task.Delay(GlobalDatabase.SharedRandom.Next(10, 100));

        var conditions = new[] { "Sunny", "Cloudy", "Raining Cats and Dogs", "Foggy", "Apocalyptic", "Vibing" };
        var emojis = new[] { "☀️", "☁️", "🌧️", "🌫️", "🔥", "✨" };
        var advice = new[] {
            "Bring an umbrella (or don't, I'm not your mom)",
            "Perfect weather for debugging",
            "Stay inside and refactor something",
            "Mercury is in retrograde, don't deploy",
            "Good day to push to production on Friday",
        };

        var idx = GlobalDatabase.SharedRandom.Next(conditions.Length);
        var temp = GlobalDatabase.SharedRandom.Next(-20, 45);

        return new WeatherData
        {
            city = city,
            CITY = city.ToUpper(),
            temp = temp,
            tempF = temp * 9 / 5 + 32,
            tempK = temp + 273.15,
            tempR = (temp + 273.15) * 9 / 5, // Rankine for the cultured
            condition = conditions[idx],
            conditionEmoji = emojis[idx],
            advice = advice[GlobalDatabase.SharedRandom.Next(advice.Length)],
            chaosLevel = GlobalDatabase.SharedRandom.Next(1, 11),
            isReal = false,
            source = "TotallyRealWeatherAPI™ (it's not real)",
            timestamp = DateTime.UtcNow,
            warnings = new List<string> { "This data is 100% made up", "Do not make life decisions based on this" },
            forecast = GetRandomForecast(),
        };
    }

    private string GetRandomForecast()
    {
        var forecasts = new[] {
            "Tomorrow: Same as today, but slightly worse",
            "Next week: Expect weather. Definitely weather.",
            "This weekend: 60% chance of regretting something",
            "Tonight: Dark, with periods of darkness",
        };
        return forecasts[GlobalDatabase.SharedRandom.Next(forecasts.Length)];
    }

    /// <summary>
    /// INLINE CSS HELL PAGE
    /// Features: Every element has inline styles, Comic Sans, blink simulation, rainbow chaos
    /// Anti-patterns: Inline Styles Everywhere, No CSS Classes, Accessibility Nightmare
    /// </summary>
    public IActionResult InlineCssHell(int chaos = 5)
    {
        chaos = Math.Clamp(chaos, 1, 11); // These go to 11

        var model = new InlineCssModel
        {
            title = "Welcome to CSS Hell 🔥",
            content = "Where style attributes go to party and best practices go to die",
            fontFamily = StyleGenerator.GetRandomFont(),
            backgroundColor = StyleGenerator.GetRandomColor(),
            textColor = StyleGenerator.GetRandomColor(),
            fontSize = GlobalDatabase.SharedRandom.Next(12, 48),
            blinkSpeed = GlobalDatabase.SharedRandom.Next(100, 1000),
            enableChaos = chaos > 5,
            marqueeText = "🎉 THIS TEXT IS SCROLLING LIKE IT'S 1999 🎉",
            rotationDegrees = GlobalDatabase.SharedRandom.Next(-15, 15),
            items = new List<InlineCssItem>()
        };

        // Generate items with random inline styles
        var itemTexts = new[] {
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
                style = StyleGenerator.GenerateChaosStyle(),
                onclick = $"alert('You clicked item {i + 1}! Your reward: nothing.');",
                isImportant = true
            });
        }

        ViewBag.Model = model;
        ViewBag.ChaosLevel = chaos;
        ViewBag.PageLoadTime = DateTime.Now.Ticks;
        ViewBag.RandomStyles = Enumerable.Range(0, 20).Select(_ => StyleGenerator.GenerateChaosStyle()).ToList();

        return View();
    }
}

/// <summary>
/// Global Database - Because who needs actual databases when you have static variables?
/// Thread safety? Never heard of her.
/// </summary>
public static class GlobalDatabase
{
    public static string LastQuery = "";
    public static int QueryCount = 0;
    public static int UnicornCount = 0;
    public static Dictionary<string, object> Cache = new Dictionary<string, object>();
    public static List<string> SqlQueryLog = new List<string>(); // for "auditing"
    public static Dictionary<string, int> PageViews = new Dictionary<string, int>();
    public static List<GodObjectProfile> UserProfiles = new List<GodObjectProfile>();
    public static StringBuilder GlobalHtmlBuilder = new StringBuilder(); // shared HTML builder
    public static Random SharedRandom = new Random(); // one random to rule them all
    public static Exception LastException = null; // save for later
    public static bool IsEverythingFine = true; // narrator: it was not fine
    // TODO: Add more static state because this is fine
}
