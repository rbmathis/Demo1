using Demo1.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="InMemoryUserProfileService"/> verifying profile CRUD operations, field validation, and statistics.
/// </summary>
public class InMemoryUserProfileServiceTests
{
    private readonly InMemoryUserProfileService _service;

    public InMemoryUserProfileServiceTests()
    {
        var logger = Mock.Of<ILogger<InMemoryUserProfileService>>();
        _service = new InMemoryUserProfileService(logger);
    }

    /// <summary>
    /// Verifies that requesting a profile with an empty userId returns the default "Demo User" profile.
    /// </summary>
    [Fact]
    public async Task GetProfileAsync_DefaultUser_ReturnsPresetProfile()
    {
        // Arrange
        var userId = "";

        // Act
        var profile = await _service.GetProfileAsync(userId);

        // Assert
        Assert.Equal("Demo User", profile.Name);
    }

    /// <summary>
    /// Verifies that the default user profile has the expected email address.
    /// </summary>
    [Fact]
    public async Task GetProfileAsync_DefaultUser_HasCorrectEmail()
    {
        // Arrange
        var userId = "";

        // Act
        var profile = await _service.GetProfileAsync(userId);

        // Assert
        Assert.Equal("demo@example.com", profile.Email);
    }

    /// <summary>
    /// Verifies that requesting a non-existent user creates a new profile with that ID.
    /// </summary>
    [Fact]
    public async Task GetProfileAsync_NonExistentUser_CreatesNew()
    {
        // Arrange
        var userId = "new-user-123";

        // Act
        var profile = await _service.GetProfileAsync(userId);

        // Assert
        Assert.NotNull(profile);
        Assert.Equal("new-user-123", profile.Id);
    }

    /// <summary>
    /// Verifies that two calls with the same user ID return the same profile instance.
    /// </summary>
    [Fact]
    public async Task GetProfileAsync_SameUser_ReturnsSameProfile()
    {
        // Arrange
        var userId = "consistent-user";

        // Act
        var profile1 = await _service.GetProfileAsync(userId);
        var profile2 = await _service.GetProfileAsync(userId);

        // Assert
        Assert.Same(profile1, profile2);
    }

    /// <summary>
    /// Verifies that updating the Name field persists the new value.
    /// </summary>
    [Fact]
    public async Task UpdateFieldAsync_ValidField_UpdatesValue()
    {
        // Arrange
        var userId = "default-user";

        // Act
        var profile = await _service.UpdateFieldAsync(userId, "Name", "New Name");

        // Assert
        Assert.Equal("New Name", profile.Name);
    }

    /// <summary>
    /// Verifies that updating the Email field persists the new value.
    /// </summary>
    [Fact]
    public async Task UpdateFieldAsync_Email_UpdatesValue()
    {
        // Arrange
        var userId = "default-user";

        // Act
        var profile = await _service.UpdateFieldAsync(userId, "Email", "new@test.com");

        // Assert
        Assert.Equal("new@test.com", profile.Email);
    }

    /// <summary>
    /// Verifies that updating the Age field converts the string value to an integer.
    /// </summary>
    [Fact]
    public async Task UpdateFieldAsync_Age_UpdatesValue()
    {
        // Arrange
        var userId = "default-user";

        // Act
        var profile = await _service.UpdateFieldAsync(userId, "Age", "30");

        // Assert
        Assert.Equal(30, profile.Age);
    }

    /// <summary>
    /// Verifies that attempting to update a disallowed field throws ArgumentException.
    /// </summary>
    [Fact]
    public async Task UpdateFieldAsync_InvalidField_ThrowsArgumentException()
    {
        // Arrange
        var userId = "default-user";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateFieldAsync(userId, "Password", "secret"));
    }

    /// <summary>
    /// Verifies that the allowed fields validation is case-insensitive (no exception for lowercase field names).
    /// </summary>
    [Fact]
    public async Task UpdateFieldAsync_FieldIsCaseInsensitive()
    {
        // Arrange
        var userId = "default-user";

        // Act — "name" passes the AllowedFields check (case-insensitive HashSet)
        var exception = await Record.ExceptionAsync(
            () => _service.UpdateFieldAsync(userId, "name", "Case Test"));

        // Assert — no ArgumentException thrown, proving AllowedFields is case-insensitive
        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that UpdatedAt timestamp is refreshed after a field update.
    /// </summary>
    [Fact]
    public async Task UpdateFieldAsync_UpdatesTimestamp()
    {
        // Arrange
        var userId = "default-user";
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var profile = await _service.UpdateFieldAsync(userId, "City", "Portland");

        // Assert
        Assert.True(profile.UpdatedAt >= before);
        Assert.True(profile.UpdatedAt <= DateTime.UtcNow.AddSeconds(1));
    }

    /// <summary>
    /// Verifies that a fresh service instance reports exactly one profile (the default).
    /// </summary>
    [Fact]
    public void GetStats_DefaultState_HasOneProfile()
    {
        // Arrange & Act
        var stats = _service.GetStats();

        // Assert
        Assert.Equal(1, stats.TotalProfiles);
    }

    /// <summary>
    /// Verifies that creating a new user profile increments the total profile count.
    /// </summary>
    [Fact]
    public async Task GetStats_AfterNewProfile_IncrementsCount()
    {
        // Arrange
        await _service.GetProfileAsync("brand-new-user");

        // Act
        var stats = _service.GetStats();

        // Assert
        Assert.Equal(2, stats.TotalProfiles);
    }

    /// <summary>
    /// Verifies that all profiles are active by default, so ActiveProfiles matches TotalProfiles.
    /// </summary>
    [Fact]
    public async Task GetStats_ActiveProfiles_CountsCorrectly()
    {
        // Arrange
        await _service.GetProfileAsync("active-user-1");
        await _service.GetProfileAsync("active-user-2");

        // Act
        var stats = _service.GetStats();

        // Assert
        Assert.Equal(stats.TotalProfiles, stats.ActiveProfiles);
    }

    /// <summary>
    /// Verifies that a null userId resolves to the default profile.
    /// </summary>
    [Fact]
    public async Task GetProfileAsync_NullUserId_ReturnsDefaultProfile()
    {
        // Arrange & Act
        var profile = await _service.GetProfileAsync(null!);

        // Assert
        Assert.Equal("Demo User", profile.Name);
        Assert.Equal("demo@example.com", profile.Email);
    }
}
