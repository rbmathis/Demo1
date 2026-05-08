using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Demo1.Controllers;

/// <summary>
/// Provides cookie-based authentication actions for the admin dashboard.
/// Credentials are read from configuration (<c>AdminDashboard:Username</c> and
/// <c>AdminDashboard:Password</c>) and should be supplied via environment variables
/// (<c>ADMINDASHBOARD__USERNAME</c> / <c>ADMINDASHBOARD__PASSWORD</c>) in production.
/// </summary>
public class AdminAuthController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminAuthController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminAuthController"/>.
    /// </summary>
    /// <param name="configuration">Application configuration for reading admin credentials.</param>
    /// <param name="logger">Logger instance.</param>
    public AdminAuthController(IConfiguration configuration, ILogger<AdminAuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Displays the admin login form.
    /// </summary>
    /// <param name="returnUrl">Optional URL to redirect to after successful login.</param>
    /// <returns>The Login view.</returns>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Validates the submitted credentials and, on success, issues an admin cookie and redirects.
    /// </summary>
    /// <param name="username">The submitted username.</param>
    /// <param name="password">The submitted password.</param>
    /// <param name="returnUrl">Optional URL to redirect to after login.</param>
    /// <returns>A redirect on success; the Login view with an error on failure.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        var configUsername = _configuration["AdminDashboard:Username"] ?? string.Empty;
        var configPassword = _configuration["AdminDashboard:Password"] ?? string.Empty;

        if (string.IsNullOrEmpty(configPassword))
        {
            _logger.LogWarning("Admin login attempt rejected — AdminDashboard:Password is not configured");
            ModelState.AddModelError(string.Empty, "Admin login is not configured on this server.");
            return View();
        }

        var usernameMatch = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(username ?? string.Empty),
            Encoding.UTF8.GetBytes(configUsername));

        var passwordMatch = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(password ?? string.Empty),
            Encoding.UTF8.GetBytes(configPassword));

        if (!usernameMatch || !passwordMatch)
        {
            _logger.LogWarning("Failed admin login attempt for username: {Username}", SanitizeForLog(username));
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View();
        }

        _logger.LogInformation("Admin user '{Username}' signed in", SanitizeForLog(configUsername));

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, configUsername),
            new(ClaimTypes.Role, "Admin"),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = false });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "FeatureFlag");
    }

    /// <summary>
    /// Signs out the current admin user and redirects to the home page.
    /// </summary>
    /// <returns>A redirect to the home page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var username = User.Identity?.Name ?? "unknown";
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("Admin user '{Username}' signed out", SanitizeForLog(username));
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Strips newline and carriage-return characters from a value before it is written to a log sink,
    /// preventing log-forging via user-supplied input.
    /// </summary>
    private static string SanitizeForLog(string? value) =>
        value?.ReplaceLineEndings(" ").Trim() ?? string.Empty;
}
