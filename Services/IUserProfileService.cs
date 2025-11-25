namespace Demo1.Services;

/// <summary>
/// Manages user profile data with proper encapsulation.
/// Replaces the anti-pattern GodObjectHelper static class.
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <returns>User profile data.</returns>
    Task<UserProfile> GetProfileAsync(string userId);

    /// <summary>
    /// Updates a user profile field.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="fieldName">Field to update.</param>
    /// <param name="value">New value.</param>
    /// <returns>Updated profile.</returns>
    Task<UserProfile> UpdateFieldAsync(string userId, string fieldName, string value);

    /// <summary>
    /// Gets profile statistics.
    /// </summary>
    ProfileStats GetStats();
}

/// <summary>
/// Represents a user profile with proper encapsulation.
/// </summary>
public class UserProfile
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Default User";
    public string Email { get; set; } = "user@example.com";
    public int Age { get; set; } = 25;
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Country { get; set; } = "";
    public string FavoriteColor { get; set; } = "";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Note: Sensitive data like passwords should NEVER be stored in a profile model
    // Authentication should be handled by a proper identity service
}

/// <summary>
/// Statistics about profile service usage.
/// </summary>
public class ProfileStats
{
    public int TotalProfiles { get; init; }
    public int ActiveProfiles { get; init; }
}

/// <summary>
/// In-memory implementation of user profile service for demo purposes.
/// In production, this would use Entity Framework with proper data protection.
/// </summary>
public class InMemoryUserProfileService : IUserProfileService
{
    private readonly ILogger<InMemoryUserProfileService> _logger;
    private readonly Dictionary<string, UserProfile> _profiles = new();
    private readonly object _lock = new();
    private const string DefaultUserId = "default-user";

    private static readonly HashSet<string> AllowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(UserProfile.Name),
        nameof(UserProfile.Email),
        nameof(UserProfile.Age),
        nameof(UserProfile.City),
        nameof(UserProfile.State),
        nameof(UserProfile.Country),
        nameof(UserProfile.FavoriteColor),
    };

    public InMemoryUserProfileService(ILogger<InMemoryUserProfileService> logger)
    {
        _logger = logger;

        // Create a default user
        _profiles[DefaultUserId] = new UserProfile
        {
            Id = DefaultUserId,
            Name = "Demo User",
            Email = "demo@example.com",
            Age = 25,
            City = "Seattle",
            State = "WA",
            Country = "USA",
            FavoriteColor = "Blue",
        };
    }

    public Task<UserProfile> GetProfileAsync(string userId)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = DefaultUserId;
            }

            if (!_profiles.TryGetValue(userId, out var profile))
            {
                profile = new UserProfile { Id = userId };
                _profiles[userId] = profile;
            }

            return Task.FromResult(profile);
        }
    }

    public Task<UserProfile> UpdateFieldAsync(string userId, string fieldName, string value)
    {
        _logger.LogInformation("Updating field {Field} for user {UserId}", fieldName, userId);

        if (!AllowedFields.Contains(fieldName))
        {
            throw new ArgumentException($"Field '{fieldName}' is not updatable.", nameof(fieldName));
        }

        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = DefaultUserId;
            }

            if (!_profiles.TryGetValue(userId, out var profile))
            {
                profile = new UserProfile { Id = userId };
                _profiles[userId] = profile;
            }

            var property = typeof(UserProfile).GetProperty(fieldName);
            if (property != null && property.CanWrite)
            {
                var convertedValue = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(profile, convertedValue);
                profile.UpdatedAt = DateTime.UtcNow;
            }

            return Task.FromResult(profile);
        }
    }

    public ProfileStats GetStats()
    {
        lock (_lock)
        {
            return new ProfileStats
            {
                TotalProfiles = _profiles.Count,
                ActiveProfiles = _profiles.Values.Count(p => p.IsActive),
            };
        }
    }
}
