using Microsoft.AspNetCore.Mvc;
using Demo1.Models;
using Demo1.Services;

namespace Demo1.Controllers;

/// <summary>
/// Controller for the Security Headers Playground — an interactive lab
/// where users can toggle security headers and observe attack behaviors.
/// </summary>
public class SecurityLabController : Controller
{
    private readonly ISecurityLabService _securityLabService;
    private readonly ILogger<SecurityLabController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityLabController"/> class.
    /// </summary>
    /// <param name="securityLabService">The security lab service for header management.</param>
    /// <param name="logger">The logger instance.</param>
    public SecurityLabController(ISecurityLabService securityLabService, ILogger<SecurityLabController> logger)
    {
        _securityLabService = securityLabService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the Security Lab playground with header toggles and attack scenarios.
    /// </summary>
    /// <returns>The Security Lab index view.</returns>
    [HttpGet]
    public IActionResult Index()
    {
        var viewModel = new SecurityLabViewModel
        {
            HeaderStates = _securityLabService.GetHeaderStates(),
            AttackScenarios = _securityLabService.GetAttackScenarios(),
            ProtectionScore = _securityLabService.GetProtectionScore()
        };

        return View(viewModel);
    }

    /// <summary>
    /// Configures a security header's enabled state via AJAX.
    /// </summary>
    /// <param name="request">The header configuration request.</param>
    /// <returns>JSON response with updated protection score and header states.</returns>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult Configure([FromBody] HeaderConfigRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Header))
            return BadRequest(new { error = "Header name is required" });

        _securityLabService.SetHeaderState(request.Header, request.Enabled);
        _logger.LogInformation("Security Lab: Header {Header} set to {Enabled}", request.Header, request.Enabled);

        return Json(new
        {
            protectionScore = _securityLabService.GetProtectionScore(),
            headerStates = _securityLabService.GetHeaderStates()
        });
    }

    /// <summary>
    /// Resets all security headers to their default (enabled) state.
    /// </summary>
    /// <returns>JSON response with reset header states and protection score.</returns>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult Reset()
    {
        _securityLabService.ResetToDefaults();
        _logger.LogInformation("Security Lab: All headers reset to defaults");

        return Json(new
        {
            protectionScore = _securityLabService.GetProtectionScore(),
            headerStates = _securityLabService.GetHeaderStates()
        });
    }

    /// <summary>
    /// Renders the victim page in an iframe with current header configuration applied.
    /// This page is intentionally minimal and rendered without the main layout.
    /// </summary>
    /// <returns>The victim page view without layout.</returns>
    [HttpGet]
    public IActionResult VictimPage()
    {
        return View();
    }

    /// <summary>
    /// Returns attack scenario information for the specified attack type.
    /// </summary>
    /// <param name="type">The attack type identifier (XSS, Clickjacking, MimeSniff).</param>
    /// <returns>JSON response with attack scenario details.</returns>
    [HttpGet]
    public IActionResult Attack(string type)
    {
        var scenarios = _securityLabService.GetAttackScenarios();
        var scenario = scenarios.FirstOrDefault(s => s.Type.ToString().Equals(type, StringComparison.OrdinalIgnoreCase));

        if (scenario == null)
            return NotFound(new { error = $"Attack type '{type}' not found" });

        return Json(scenario);
    }
}

/// <summary>
/// Request model for header configuration AJAX calls.
/// </summary>
public class HeaderConfigRequest
{
    /// <summary>Gets or sets the header name to configure.</summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the header should be enabled.</summary>
    public bool Enabled { get; set; }
}
