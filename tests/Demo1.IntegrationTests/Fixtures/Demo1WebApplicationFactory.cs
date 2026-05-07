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
    private SqliteConnection? _connection;

    /// <summary>
    /// Configures the web host for testing purposes.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Create a shared in-memory SQLite connection that stays open for the test lifetime
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

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

            // Add SQLite in-memory database using the shared connection
            services.AddDbContext<AchievementDbContext>(options =>
                options.UseSqlite(_connection));
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
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
