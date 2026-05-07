using Demo1.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Demo1.IntegrationTests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory for integration testing the Demo1 application.
/// Sets the environment to "Testing" to avoid HTTPS redirect issues during tests.
/// Replaces the SQLite database with an in-memory database for test isolation.
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
