namespace Demo1.IntegrationTests.Fixtures;

/// <summary>
/// Defines a shared test collection so all integration tests reuse the same
/// <see cref="Demo1WebApplicationFactory"/> instance, improving test performance.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<Demo1WebApplicationFactory>
{
}
