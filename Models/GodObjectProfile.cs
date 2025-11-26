// Profile model - kept for backward compatibility with views
// The proper model is now UserProfile in Services/IUserProfileService.cs

namespace Demo1.Models;

/// <summary>
/// Legacy profile model - demonstrates what NOT to do.
/// Use <see cref="Demo1.Services.UserProfile"/> for proper implementation.
/// </summary>
public class GodObjectProfile
{
    // Basic user info
    public string name { get; set; } = string.Empty;
    public string Name2 { get; set; } = string.Empty;
    public string NAME { get; set; } = string.Empty;
    public string n { get; set; } = string.Empty;
    public int age { get; set; }
    public int Age { get; set; }
    public string email { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EMAIL { get; set; } = string.Empty;

    // NOTE: Passwords and sensitive data should NEVER be stored in models
    // These properties are kept only for demonstration purposes
    public string password { get; set; } = "[REDACTED]";
    public string Password { get; set; } = "[REDACTED]";
    public string PasswordConfirm { get; set; } = "[REDACTED]";
    public string PasswordHint { get; set; } = "[REDACTED]";
    public string ssn { get; set; } = "[REDACTED]";
    public string creditCard { get; set; } = "[REDACTED]";
    public string creditCardCvv { get; set; } = "[REDACTED]";
    public string creditCardPin { get; set; } = "[REDACTED]";

    // Address
    public string address1 { get; set; } = string.Empty;
    public string city { get; set; } = string.Empty;
    public string state { get; set; } = string.Empty;
    public string zip { get; set; } = string.Empty;
    public string country { get; set; } = string.Empty;

    // Fun properties (for demo)
    public string favoriteColor { get; set; } = string.Empty;
    public int numberOfPets { get; set; }
    public string hogwartsHouse { get; set; } = string.Empty;
    public int midichlorianCount { get; set; }

    // Status
    public bool isActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string GetDisplayName() => name ?? Name2 ?? NAME ?? "Unknown User";

    public bool IsValid() => !string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(email);

    public string ToJson() => System.Text.Json.JsonSerializer.Serialize(new
    {
        name,
        email,
        age,
        city,
        state,
        country,
        favoriteColor,
        hogwartsHouse,
        isActive
    });
}
