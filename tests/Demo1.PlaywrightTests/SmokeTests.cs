using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Demo1.PlaywrightTests.Infrastructure;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Demo1.PlaywrightTests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class SmokeTests : PageTest
{
    private static Demo1ServerFixture? _server;

    [OneTimeSetUp]
    public static async Task OneTimeSetup()
    {
        // Many developer machines (and local devcontainers) don't have the
        // native browser dependencies required by Playwright. The CI runner
        // (GitHub Actions) provides these. Skip the browser tests when not
        // running in CI to avoid failing local runs.
        var runningInGithubActions = string.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), "true", StringComparison.OrdinalIgnoreCase);
        var runningInCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) || runningInGithubActions;
        if (!runningInCI)
        {
            Assert.Ignore("Playwright tests skipped when running locally (missing browser host dependencies). They run in CI.");
        }

        await PlaywrightInstaller.EnsureBrowsersAsync().ConfigureAwait(false);

        _server = new Demo1ServerFixture();
        await _server.StartAsync().ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public static async Task OneTimeTearDown()
    {
        if (_server is not null)
        {
            await _server.DisposeAsync().ConfigureAwait(false);
        }
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        if (_server is null)
        {
            throw new InvalidOperationException("Server fixture was not initialized.");
        }

        return new()
        {
            BaseURL = _server.BaseAddress.ToString(),
            IgnoreHTTPSErrors = true
        };
    }

    [Test]
    public async Task HomePage_LoadsAndDisplaysWelcomeHeading()
    {
        await Page.GotoAsync("/").ConfigureAwait(false);

        await Microsoft.Playwright.Assertions.Expect(Page).ToHaveTitleAsync("Home Page - _").ConfigureAwait(false);
        var welcomeHeading = Page.GetByRole(AriaRole.Heading, new() { Level = 1 });
        await Microsoft.Playwright.Assertions.Expect(welcomeHeading).ToHaveTextAsync("Welcome").ConfigureAwait(false);
    }

    [Test]
    public async Task Navigation_PrivacyLinkNavigatesToPrivacyPolicy()
    {
        await Page.GotoAsync("/").ConfigureAwait(false);
        var privacyLink = Page.GetByRole(AriaRole.Link, new() { Name = "Privacy" });
        await privacyLink.First.ClickAsync().ConfigureAwait(false);

        await Microsoft.Playwright.Assertions.Expect(Page).ToHaveURLAsync(new Regex("/Home/Privacy$", RegexOptions.IgnoreCase)).ConfigureAwait(false);
        var privacyHeading = Page.GetByRole(AriaRole.Heading, new() { Name = "Privacy Policy" });
        await Microsoft.Playwright.Assertions.Expect(privacyHeading).ToBeVisibleAsync().ConfigureAwait(false);
    }
}
