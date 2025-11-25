// Search result model for demonstration purposes

namespace Demo1.Models;

/// <summary>
/// Represents a search result item.
/// </summary>
public class SearchResult
{
    public int id { get; set; }
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public string category { get; set; } = string.Empty;
    public double relevance { get; set; }
    public DateTime foundAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents search query parameters.
/// </summary>
public class SearchQuery
{
    public string term { get; set; } = string.Empty;
    public string table { get; set; } = "documents";
    public string orderBy { get; set; } = "id";
    public int limit { get; set; } = 10;
    public bool unsafeMode { get; set; } = false;
    public string customWhere { get; set; } = string.Empty;
}
