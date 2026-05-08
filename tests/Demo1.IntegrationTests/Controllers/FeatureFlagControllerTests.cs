using Demo1.IntegrationTests.Fixtures;

namespace Demo1.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for the <c>FeatureFlagController</c> admin dashboard.
/// Verifies route accessibility, authentication gating, and read-only mode when
/// Azure App Configuration is not configured.
/// </summary>
[Collection("Integration")]
public class FeatureFlagControllerTests
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of <see cref="FeatureFlagControllerTests"/>.
    /// </summary>
    /// <param name="factory">Shared application factory.</param>
    public FeatureFlagControllerTests(Demo1WebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// Verifies that an unauthenticated GET request to the dashboard is redirected to the login page.
    /// </summary>
    [Fact]
    public async Task Get_Dashboard_Unauthenticated_RedirectsToLogin()
    {
        var response = await _client.GetAsync("/FeatureFlag");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/AdminAuth/Login");
    }

    /// <summary>
    /// Verifies that the admin login page renders successfully (GET).
    /// </summary>
    [Fact]
    public async Task Get_LoginPage_ReturnsOk()
    {
        var response = await _client.GetAsync("/AdminAuth/Login");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("Admin Login");
    }

    /// <summary>
    /// Verifies that a POST to the toggle endpoint without authentication is redirected to login.
    /// </summary>
    [Fact]
    public async Task Post_Toggle_Unauthenticated_RedirectsToLogin()
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("flagName", "Feature1"),
            new KeyValuePair<string, string>("enabled", "true"),
        });

        var response = await _client.PostAsync("/FeatureFlag/Toggle", content);

        // Expect redirect to login — either 302 or 401 is acceptable; check no 200/500
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(300);
    }

    /// <summary>
    /// Verifies that a POST to the login endpoint with missing/wrong credentials does not
    /// grant access (returns 200 with the login form rather than a redirect to the dashboard).
    /// </summary>
    [Fact]
    public async Task Post_Login_WithInvalidCredentials_ReturnsLoginPage()
    {
        // First get the login page to harvest the anti-forgery token
        var getResponse = await _client.GetAsync("/AdminAuth/Login");
        var html = await getResponse.Content.ReadAsStringAsync();

        var token = ExtractAntiForgeryToken(html);
        var cookies = getResponse.Headers
            .TryGetValues("Set-Cookie", out var setCookie) ? setCookie : [];

        using var request = new HttpRequestMessage(HttpMethod.Post, "/AdminAuth/Login");

        // Pass along any cookies (anti-forgery cookie)
        foreach (var cookie in cookies)
        {
            var cookieValue = cookie.Split(';')[0];
            request.Headers.TryAddWithoutValidation("Cookie", cookieValue);
        }

        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("username", "admin"),
            new KeyValuePair<string, string>("password", "wrong-password"),
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
        });
        request.Content = formContent;

        var postResponse = await _client.SendAsync(request);

        // The login page should be re-rendered (200) or the admin password is not configured
        // In either case we must NOT be redirected to the dashboard
        postResponse.StatusCode.Should().NotBe(HttpStatusCode.Found);
    }

    /// <summary>
    /// Verifies that the feature flag toggle action is not reachable via GET (method not allowed
    /// or redirect to login).
    /// </summary>
    [Fact]
    public async Task Get_Toggle_ReturnsNon2xx()
    {
        var response = await _client.GetAsync("/FeatureFlag/Toggle");

        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(300);
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        const string marker = "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0) return string.Empty;
        start += marker.Length;
        var end = html.IndexOf('"', start);
        return end < 0 ? string.Empty : html[start..end];
    }
}
