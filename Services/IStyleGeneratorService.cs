namespace Demo1.Services;

/// <summary>
/// Generates random styles for the CSS demo page.
/// Replaces the static StyleGenerator class with proper DI.
/// </summary>
public interface IStyleGeneratorService
{
    /// <summary>
    /// Gets a random font family.
    /// </summary>
    string GetRandomFont();

    /// <summary>
    /// Gets a random color in hex format.
    /// </summary>
    string GetRandomColor();

    /// <summary>
    /// Generates a random chaos style string.
    /// </summary>
    string GenerateChaosStyle();
}

/// <summary>
/// Implementation of style generator service.
/// </summary>
public class StyleGeneratorService : IStyleGeneratorService
{
    private readonly Random _random = new();

    private static readonly string[] Fonts =
    {
        "Comic Sans MS",
        "Papyrus",
        "Impact",
        "Courier New",
        "Arial Black",
        "Times New Roman",
        "Verdana",
        "Georgia",
    };

    private static readonly string[] Colors =
    {
        "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4", "#FFEAA7",
        "#DDA0DD", "#98D8C8", "#F7DC6F", "#BB8FCE", "#85C1E9",
        "#FF69B4", "#00FF00", "#FF4500", "#1E90FF", "#FFD700",
    };

    public string GetRandomFont()
    {
        return Fonts[_random.Next(Fonts.Length)];
    }

    public string GetRandomColor()
    {
        return Colors[_random.Next(Colors.Length)];
    }

    public string GenerateChaosStyle()
    {
        var styles = new List<string>
        {
            $"font-family: '{GetRandomFont()}'",
            $"color: {GetRandomColor()}",
            $"background-color: {GetRandomColor()}",
            $"font-size: {_random.Next(10, 36)}px",
            $"padding: {_random.Next(5, 20)}px",
            $"margin: {_random.Next(2, 15)}px",
            $"border-radius: {_random.Next(0, 25)}px",
            $"transform: rotate({_random.Next(-10, 10)}deg)",
        };

        // Randomly include some styles
        var selectedStyles = styles
            .OrderBy(_ => _random.Next())
            .Take(_random.Next(3, 6))
            .ToList();

        return string.Join("; ", selectedStyles);
    }
}
