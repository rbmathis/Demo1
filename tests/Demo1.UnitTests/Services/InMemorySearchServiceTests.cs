using Demo1.Models;
using Demo1.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="InMemorySearchService"/> verifying search filtering, query history, and concurrency safety.
/// </summary>
public class InMemorySearchServiceTests
{
    private readonly InMemorySearchService _service;

    public InMemorySearchServiceTests()
    {
        var logger = Mock.Of<ILogger<InMemorySearchService>>();
        _service = new InMemorySearchService(logger);
    }

    /// <summary>
    /// Verifies that searching for "Document" returns the 3 items containing that word in title or description.
    /// </summary>
    [Fact]
    public async Task SearchAsync_WithMatchingTerm_ReturnsResults()
    {
        // Arrange
        var query = new SearchQuery { term = "Document" };

        // Act
        var results = await _service.SearchAsync(query);

        // Assert
        Assert.Equal(3, results.Count);
    }

    /// <summary>
    /// Verifies that a term matching no titles or descriptions returns an empty list.
    /// </summary>
    [Fact]
    public async Task SearchAsync_WithNoMatch_ReturnsEmptyList()
    {
        // Arrange
        var query = new SearchQuery { term = "xyz123" };

        // Act
        var results = await _service.SearchAsync(query);

        // Assert
        Assert.Empty(results);
    }

    /// <summary>
    /// Verifies that an empty search term returns all sample data (no filtering applied).
    /// </summary>
    [Fact]
    public async Task SearchAsync_WithEmptyTerm_ReturnsAllResults()
    {
        // Arrange
        var query = new SearchQuery { term = "" };

        // Act
        var results = await _service.SearchAsync(query);

        // Assert
        Assert.Equal(5, results.Count);
    }

    /// <summary>
    /// Verifies that a null search term returns all sample data (treated as no filter).
    /// </summary>
    [Fact]
    public async Task SearchAsync_WithNullTerm_ReturnsAllResults()
    {
        // Arrange
        var query = new SearchQuery { term = null! };

        // Act
        var results = await _service.SearchAsync(query);

        // Assert
        Assert.Equal(5, results.Count);
    }

    /// <summary>
    /// Verifies that a whitespace-only term returns all sample data.
    /// </summary>
    [Fact]
    public async Task SearchAsync_WithWhitespaceTerm_ReturnsAllResults()
    {
        // Arrange
        var query = new SearchQuery { term = "   " };

        // Act
        var results = await _service.SearchAsync(query);

        // Assert
        Assert.Equal(5, results.Count);
    }

    /// <summary>
    /// Verifies that search matching is case-insensitive.
    /// </summary>
    [Fact]
    public async Task SearchAsync_IsCaseInsensitive()
    {
        // Arrange
        var lowerQuery = new SearchQuery { term = "document" };
        var upperQuery = new SearchQuery { term = "DOCUMENT" };

        // Act
        var lowerResults = await _service.SearchAsync(lowerQuery);
        var upperResults = await _service.SearchAsync(upperQuery);

        // Assert
        Assert.Equal(lowerResults.Count, upperResults.Count);
        Assert.Equal(3, lowerResults.Count);
    }

    /// <summary>
    /// Verifies that search matches against the description field as well as title.
    /// </summary>
    [Fact]
    public async Task SearchAsync_MatchesDescription()
    {
        // Arrange
        var query = new SearchQuery { term = "Quarterly" };

        // Act
        var results = await _service.SearchAsync(query);

        // Assert
        Assert.Single(results);
        Assert.Equal("Report Alpha", results[0].title);
    }

    /// <summary>
    /// Verifies that special characters in the search term do not cause errors and return empty results.
    /// </summary>
    [Fact]
    public async Task SearchAsync_WithSpecialCharacters_ReturnsEmpty()
    {
        // Arrange
        var query = new SearchQuery { term = "'; DROP TABLE" };

        // Act
        var results = await _service.SearchAsync(query);

        // Assert
        Assert.Empty(results);
    }

    /// <summary>
    /// Verifies that TotalQueryCount increments correctly after each search.
    /// </summary>
    [Fact]
    public async Task TotalQueryCount_IncrementsAfterSearch()
    {
        // Arrange
        Assert.Equal(0, _service.TotalQueryCount);

        // Act
        await _service.SearchAsync(new SearchQuery { term = "a" });
        await _service.SearchAsync(new SearchQuery { term = "b" });
        await _service.SearchAsync(new SearchQuery { term = "c" });

        // Assert
        Assert.Equal(3, _service.TotalQueryCount);
    }

    /// <summary>
    /// Verifies that GetRecentQueries returns the correct number of history entries.
    /// </summary>
    [Fact]
    public async Task GetRecentQueries_ReturnsSearchHistory()
    {
        // Arrange
        await _service.SearchAsync(new SearchQuery { term = "alpha" });
        await _service.SearchAsync(new SearchQuery { term = "beta" });
        await _service.SearchAsync(new SearchQuery { term = "gamma" });

        // Act
        var history = _service.GetRecentQueries(10);

        // Assert
        Assert.Equal(3, history.Count);
    }

    /// <summary>
    /// Verifies that GetRecentQueries respects the count limit parameter.
    /// </summary>
    [Fact]
    public async Task GetRecentQueries_RespectsLimit()
    {
        // Arrange
        await _service.SearchAsync(new SearchQuery { term = "one" });
        await _service.SearchAsync(new SearchQuery { term = "two" });
        await _service.SearchAsync(new SearchQuery { term = "three" });
        await _service.SearchAsync(new SearchQuery { term = "four" });
        await _service.SearchAsync(new SearchQuery { term = "five" });

        // Act
        var history = _service.GetRecentQueries(2);

        // Assert
        Assert.Equal(2, history.Count);
    }

    /// <summary>
    /// Verifies that concurrent searches are handled safely without data corruption.
    /// </summary>
    [Fact]
    public async Task SearchAsync_ConcurrentSearches_HandledSafely()
    {
        // Arrange
        var tasks = Enumerable.Range(0, 10)
            .Select(i => _service.SearchAsync(new SearchQuery { term = $"term{i}" }))
            .ToArray();

        // Act
        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, _service.TotalQueryCount);
    }
}
