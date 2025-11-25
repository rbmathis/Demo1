using Demo1.Models;

namespace Demo1.Services;

/// <summary>
/// Provides search functionality with proper abstraction over data access.
/// Replaces the anti-pattern static SearchDatabase class.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Searches for results matching the query criteria.
    /// </summary>
    /// <param name="query">The search query parameters.</param>
    /// <returns>A list of search results.</returns>
    Task<List<SearchResult>> SearchAsync(SearchQuery query);

    /// <summary>
    /// Gets the recent query history for audit purposes.
    /// </summary>
    /// <param name="count">Maximum number of entries to return.</param>
    /// <returns>List of recent queries.</returns>
    IReadOnlyList<string> GetRecentQueries(int count = 10);

    /// <summary>
    /// Gets the total number of queries executed.
    /// </summary>
    int TotalQueryCount { get; }
}

/// <summary>
/// In-memory implementation of the search service for demo purposes.
/// In production, this would connect to a real database via Entity Framework or Dapper.
/// </summary>
public class InMemorySearchService : ISearchService
{
    private readonly ILogger<InMemorySearchService> _logger;
    private readonly List<string> _queryHistory = new();
    private readonly object _lock = new();
    private readonly List<SearchResult> _sampleData;

    public InMemorySearchService(ILogger<InMemorySearchService> logger)
    {
        _logger = logger;
        
        // Sample data for demo - in production this would come from a database
        _sampleData = new List<SearchResult>
        {
            new() { id = 1, title = "Document One", description = "First sample document", category = "docs" },
            new() { id = 2, title = "Document Two", description = "Second sample document", category = "docs" },
            new() { id = 3, title = "Report Alpha", description = "Quarterly report", category = "reports" },
            new() { id = 4, title = "Report Beta", description = "Annual summary", category = "reports" },
            new() { id = 5, title = "Policy Document", description = "Company policies", category = "policy" },
        };
    }

    public Task<List<SearchResult>> SearchAsync(SearchQuery query)
    {
        _logger.LogInformation("Executing search for term: {Term}", query.term);

        lock (_lock)
        {
            _queryHistory.Add($"[{DateTime.UtcNow:O}] Search: {query.term}");
        }

        var results = _sampleData.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.term))
        {
            results = results.Where(r =>
                r.title.Contains(query.term, StringComparison.OrdinalIgnoreCase) ||
                r.description.Contains(query.term, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult(results.ToList());
    }

    public IReadOnlyList<string> GetRecentQueries(int count = 10)
    {
        lock (_lock)
        {
            return _queryHistory.TakeLast(count).ToList().AsReadOnly();
        }
    }

    public int TotalQueryCount
    {
        get
        {
            lock (_lock)
            {
                return _queryHistory.Count;
            }
        }
    }
}
