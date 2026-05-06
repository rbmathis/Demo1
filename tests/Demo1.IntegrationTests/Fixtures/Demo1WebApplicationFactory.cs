using Microsoft.AspNetCore.Hosting;

namespace Demo1.IntegrationTests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory for integration testing the Demo1 application.
/// Sets the environment to "Testing" to avoid HTTPS redirect issues during tests.
/// </summary>
public class Demo1WebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Configures the web host for testing purposes.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Additional test service overrides can be added here
        });
    }
}
