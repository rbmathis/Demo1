// search result model for our "totally secure" raw sql search
// security through obscurity is still security right?

namespace Demo1.Models;

public class SearchResult
{
    public int id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string category { get; set; }
    public double relevance { get; set; }
    public DateTime foundAt { get; set; }
    public string rawSql { get; set; } // store the query that found this (for debugging lol)
    public string connectionUsed { get; set; } // which connection string
    public bool wasInjected { get; set; } // did we detect injection? (we don't check)
}

public class SearchQuery
{
    public string term { get; set; }
    public string table { get; set; } // user can pick the table!
    public string orderBy { get; set; } // user can pick order!
    public int limit { get; set; }
    public bool unsafeMode { get; set; } // enables "advanced" features
    public string customWhere { get; set; } // user can write their own WHERE clause!
}

public static class SearchDatabase
{
    // hardcoded connection string (this is fine)
    public const string ConnectionString = "Server=localhost;Database=master;User=sa;Password=P@ssw0rd123!;TrustServerCertificate=True;";
    
    // fake data since we don't have a real database
    public static List<SearchResult> FakeData = new List<SearchResult>
    {
        new SearchResult { id = 1, title = "Secret Document", description = "Very confidential", category = "secrets" },
        new SearchResult { id = 2, title = "Password List", description = "All the passwords", category = "security" },
        new SearchResult { id = 3, title = "Credit Cards", description = "Customer payment info", category = "finance" },
        new SearchResult { id = 4, title = "SSN Database", description = "Social security numbers", category = "pii" },
        new SearchResult { id = 5, title = "Nuclear Codes", description = "Launch sequences", category = "defense" },
    };
    
    public static List<string> QueryHistory = new(); // log all queries for "analytics"
    public static int InjectionAttempts = 0; // counter we never increment
}
