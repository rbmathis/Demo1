// model for the inline css hell page
// this exists purely to demonstrate what NOT to do
// if you're reading this, I'm sorry

namespace Demo1.Models;

public class InlineCssModel
{
    public string title { get; set; }
    public string content { get; set; }
    public List<InlineCssItem> items { get; set; }
    public string backgroundColor { get; set; }
    public string textColor { get; set; }
    public string fontFamily { get; set; }
    public int blinkSpeed { get; set; }
    public bool enableChaos { get; set; }
    public string marqueeText { get; set; }
    public int fontSize { get; set; }
    public string borderStyle { get; set; }
    public int rotationDegrees { get; set; }
}

public class InlineCssItem
{
    public string text { get; set; }
    public string style { get; set; } // inline style as string, obviously
    public string onclick { get; set; } // inline javascript handler
    public bool isImportant { get; set; } // adds !important to everything
}

public class StyleGenerator
{
    private static Random _rng = new Random();
    
    public static string GetRandomColor()
    {
        var colors = new[] { 
            "#FF00FF", "#00FFFF", "#FFFF00", "#FF0000", "#00FF00", 
            "#0000FF", "#FF6600", "#6600FF", "#00FF66", "hotpink",
            "limegreen", "deeppink", "chartreuse", "aquamarine"
        };
        return colors[_rng.Next(colors.Length)];
    }
    
    public static string GetRandomFont()
    {
        var fonts = new[] {
            "Comic Sans MS", "Papyrus", "Curlz MT", "Jokerman", 
            "Wingdings", "Impact", "Bradley Hand", "Chiller"
        };
        return fonts[_rng.Next(fonts.Length)];
    }
    
    public static string GenerateChaosStyle()
    {
        return $"color: {GetRandomColor()}; " +
               $"background-color: {GetRandomColor()}; " +
               $"font-family: '{GetRandomFont()}', cursive; " +
               $"font-size: {_rng.Next(8, 72)}px; " +
               $"transform: rotate({_rng.Next(-30, 30)}deg); " +
               $"text-shadow: 2px 2px {GetRandomColor()}; " +
               $"border: {_rng.Next(1, 10)}px dashed {GetRandomColor()}; " +
               $"padding: {_rng.Next(5, 30)}px; " +
               $"margin: {_rng.Next(5, 20)}px; " +
               "!important;"; // !important on everything
    }
}
