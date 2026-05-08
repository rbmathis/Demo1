using Demo1.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Demo1.IntegrationTests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory for integration testing the Demo1 application.
/// Sets the environment to "Testing" to avoid HTTPS redirect issues during tests.
/// Replaces the SQLite database with an in-memory database for test isolation.
/// Uses EF Core migrations (not EnsureCreated) to match the production migration path.
/// </summary>
public class Demo1WebApplicationFactory : WebApplicationFactory<Program>
{
    private const string SharedInMemoryConnectionString = "Data Source=Demo1IntegrationTests;Mode=Memory;Cache=Shared";
    private SqliteConnection? _keepAliveConnection;

    /// <summary>
    /// Configures the web host for testing purposes.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Keep one connection open so the shared in-memory SQLite database persists
        _keepAliveConnection = new SqliteConnection(SharedInMemoryConnectionString);
        _keepAliveConnection.Open();

        // Apply migrations on the keep-alive connection to initialize schema
        var migrateOptions = new DbContextOptionsBuilder<AchievementDbContext>()
            .UseSqlite(_keepAliveConnection)
            .Options;
        using (var migrateContext = new AchievementDbContext(migrateOptions))
        {
            migrateContext.Database.Migrate();
        }

        builder.ConfigureServices(services =>
        {
            // Remove ALL AchievementDbContext-related service descriptors
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AchievementDbContext>) ||
                    d.ServiceType == typeof(AchievementDbContext))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Use shared-cache in-memory SQLite so each DbContext gets its own connection
            services.AddDbContext<AchievementDbContext>(options =>
                options.UseSqlite(SharedInMemoryConnectionString));
        });
    }

    /// <summary>
    /// Creates a new factory with the specified feature flags enabled or disabled.
    /// Use this to test both flag-off (default) and flag-on behavior paths.
    /// </summary>
    /// <param name="flags">
    /// Dictionary of feature flag names to their enabled state.
    /// Example: <c>new Dictionary&lt;string, bool&gt; { ["DarkMode"] = true }</c>
    /// </param>
    /// <returns>A configured <see cref="WebApplicationFactory{TEntryPoint}"/> with the specified flags.</returns>
    /// <example>
    /// <code>
    /// using var flaggedFactory = factory.WithFeatureFlags(new Dictionary&lt;string, bool&gt;
    /// {
    ///     [FeatureFlags.DarkMode] = true,
    ///     [FeatureFlags.ContactForm] = false
    /// });
    /// var client = flaggedFactory.CreateClient();
    /// </code>
    /// </example>
    public WebApplicationFactory<Program> WithFeatureFlags(Dictionary<string, bool> flags)
    {
        return WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                var overrides = flags.ToDictionary(
                    kvp => $"FeatureManagement:{kvp.Key}",
                    kvp => kvp.Value.ToString().ToLowerInvariant());
                config.AddInMemoryCollection(overrides!);
            });
        });
    }

    /// <summary>
    /// Disposes the shared SQLite connection when the factory is disposed.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _keepAliveConnection?.Close();
            _keepAliveConnection?.Dispose();
        }
    }
}
