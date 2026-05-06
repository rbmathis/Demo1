using Demo1.IntegrationTests.Fixtures;

namespace Demo1.IntegrationTests.Middleware;

/// <summary>
/// Integration tests for the SecurityHeadersMiddleware.
/// Verifies that security headers are correctly applied to all responses.
/// </summary>
[Collection("Integration")]
public class SecurityHeadersTests
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityHeadersTests"/> class.
    /// </summary>
    /// <param name="factory">The shared web application factory.</param>
    public SecurityHeadersTests(Demo1WebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// Verifies the X-Content-Type-Options header is present with value "nosniff".
    /// </summary>
    [Fact]
    public async Task Response_ContainsXContentTypeOptionsHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
    }

    /// <summary>
    /// Verifies the X-Frame-Options header is present with value "DENY".
    /// </summary>
    [Fact]
    public async Task Response_ContainsXFrameOptionsHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");
    }

    /// <summary>
    /// Verifies the X-XSS-Protection header is present.
    /// </summary>
    [Fact]
    public async Task Response_ContainsXXssProtectionHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.Headers.Should().ContainKey("X-XSS-Protection");
        response.Headers.GetValues("X-XSS-Protection").Should().Contain("1; mode=block");
    }

    /// <summary>
    /// Verifies the Referrer-Policy header is present.
    /// </summary>
    [Fact]
    public async Task Response_ContainsReferrerPolicyHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.Headers.Should().ContainKey("Referrer-Policy");
        response.Headers.GetValues("Referrer-Policy").Should().Contain("strict-origin-when-cross-origin");
    }

    /// <summary>
    /// Verifies the Content-Security-Policy header is present.
    /// </summary>
    [Fact]
    public async Task Response_ContainsContentSecurityPolicyHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.Headers.Should().ContainKey("Content-Security-Policy");
        var cspValues = response.Headers.GetValues("Content-Security-Policy");
        cspValues.Should().ContainMatch("*default-src 'self'*");
    }

    /// <summary>
    /// Verifies security headers are present on non-home-page routes too.
    /// </summary>
    [Fact]
    public async Task Response_SecurityHeadersPresentOnAllRoutes()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("X-XSS-Protection");
        response.Headers.Should().ContainKey("Referrer-Policy");
    }
}
