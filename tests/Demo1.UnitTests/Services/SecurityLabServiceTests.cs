using Demo1.Services;
using Demo1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;

namespace Demo1.UnitTests.Services;

public class SecurityLabServiceTests
{
    private static (SecurityLabService service, DefaultHttpContext context) CreateService()
    {
        var context = new DefaultHttpContext();
        context.Features.Set<ISessionFeature>(new TestSessionFeature());

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(context);

        return (new SecurityLabService(accessor.Object), context);
    }

    [Fact]
    public void GetHeaderStates_ReturnsDefaults_WhenNoSessionData()
    {
        // Arrange
        var (service, _) = CreateService();

        // Act
        var states = service.GetHeaderStates();

        // Assert
        Assert.Equal(5, states.Count);
        Assert.True(states["Content-Security-Policy"]);
        Assert.True(states["X-Frame-Options"]);
        Assert.True(states["X-Content-Type-Options"]);
        Assert.True(states["X-XSS-Protection"]);
        Assert.True(states["Referrer-Policy"]);
        Assert.All(states.Values, v => Assert.True(v));
    }

    [Fact]
    public void SetHeaderState_UpdatesState()
    {
        // Arrange
        var (service, _) = CreateService();

        // Act
        service.SetHeaderState("X-Frame-Options", false);

        // Assert
        var states = service.GetHeaderStates();
        Assert.False(states["X-Frame-Options"]);
        // Other headers remain true
        Assert.True(states["Content-Security-Policy"]);
        Assert.True(states["X-Content-Type-Options"]);
        Assert.True(states["X-XSS-Protection"]);
        Assert.True(states["Referrer-Policy"]);
    }

    [Fact]
    public void GetProtectionScore_Returns100_WhenAllEnabled()
    {
        // Arrange
        var (service, _) = CreateService();

        // Act
        var score = service.GetProtectionScore();

        // Assert
        Assert.Equal(100, score);
    }

    [Fact]
    public void GetProtectionScore_ReturnsCorrectScore_WhenSomeDisabled()
    {
        // Arrange
        var (service, _) = CreateService();
        service.SetHeaderState("Content-Security-Policy", false);
        service.SetHeaderState("X-Frame-Options", false);

        // Act
        var score = service.GetProtectionScore();

        // Assert — 3 of 5 enabled = 60%
        Assert.Equal(60, score);
    }

    [Fact]
    public void GetAttackScenarios_ReturnsThreeScenarios()
    {
        // Arrange
        var (service, _) = CreateService();

        // Act
        var scenarios = service.GetAttackScenarios();

        // Assert
        Assert.Equal(3, scenarios.Count);
        Assert.Contains(scenarios, s => s.Type == AttackType.XSS);
        Assert.Contains(scenarios, s => s.Type == AttackType.Clickjacking);
        Assert.Contains(scenarios, s => s.Type == AttackType.MimeSniff);
    }

    [Fact]
    public void ResetToDefaults_ResetsAllToTrue()
    {
        // Arrange
        var (service, _) = CreateService();
        service.SetHeaderState("Content-Security-Policy", false);
        service.SetHeaderState("X-XSS-Protection", false);

        // Verify they are disabled
        var statesBefore = service.GetHeaderStates();
        Assert.False(statesBefore["Content-Security-Policy"]);
        Assert.False(statesBefore["X-XSS-Protection"]);

        // Act
        service.ResetToDefaults();

        // Assert
        var statesAfter = service.GetHeaderStates();
        Assert.All(statesAfter.Values, v => Assert.True(v));
    }

    #region Test Infrastructure

    private class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();
        public string Id => "test-session";
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _store.Keys;
        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[]? value) => _store.TryGetValue(key, out value);
    }

    private class TestSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = new TestSession();
    }

    #endregion
}
