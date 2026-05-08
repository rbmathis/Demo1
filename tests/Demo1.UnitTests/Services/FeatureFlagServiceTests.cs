using Demo1.Features;
using Demo1.Models;
using Demo1.Services;
using Demo1.UnitTests.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;

namespace Demo1.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="FeatureFlagService"/>.
/// </summary>
public class FeatureFlagServiceTests
{
    private static FeatureFlagService CreateService(
        IFeatureManager? featureManager = null,
        AzureAppConfigAdminOptions? adminOptions = null,
        ILogger<FeatureFlagService>? logger = null)
    {
        return new FeatureFlagService(
            featureManager ?? Mock.Of<IFeatureManager>(),
            adminOptions ?? new AzureAppConfigAdminOptions { IsAvailable = false },
            logger ?? Mock.Of<ILogger<FeatureFlagService>>());
    }

    [Fact]
    public void IsAzureAppConfigurationAvailable_ReturnsFalse_WhenOptionsNotAvailable()
    {
        var svc = CreateService(adminOptions: new AzureAppConfigAdminOptions { IsAvailable = false });
        Assert.False(svc.IsAzureAppConfigurationAvailable);
    }

    [Fact]
    public void IsAzureAppConfigurationAvailable_ReturnsTrue_WhenOptionsAvailable()
    {
        var svc = CreateService(adminOptions: new AzureAppConfigAdminOptions
        {
            IsAvailable = true,
            Endpoint = "https://example.azconfig.io",
        });
        Assert.True(svc.IsAzureAppConfigurationAvailable);
    }

    [Fact]
    public async Task GetFlagsAsync_ReturnsAllKnownFlags()
    {
        var mockFm = new Mock<IFeatureManager>();
        mockFm.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);
        mockFm.Setup(f => f.IsEnabledAsync(FeatureFlags.DarkMode)).ReturnsAsync(true);

        var svc = CreateService(featureManager: mockFm.Object);

        var flags = await svc.GetFlagsAsync();

        Assert.Equal(4, flags.Count);
        var darkMode = Assert.Single(flags, f => f.Name == FeatureFlags.DarkMode);
        Assert.True(darkMode.IsEnabled);
    }

    [Fact]
    public async Task GetFlagsAsync_SetsSourceToLocalConfig_WhenAzureNotAvailable()
    {
        var mockFm = new Mock<IFeatureManager>();
        mockFm.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);

        var svc = CreateService(
            featureManager: mockFm.Object,
            adminOptions: new AzureAppConfigAdminOptions { IsAvailable = false });

        var flags = await svc.GetFlagsAsync();

        Assert.All(flags, f => Assert.Equal("Local config", f.Source));
    }

    [Fact]
    public async Task GetFlagsAsync_SetsSourceToAzureAppConfiguration_WhenAvailable()
    {
        var mockFm = new Mock<IFeatureManager>();
        mockFm.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);

        var svc = CreateService(
            featureManager: mockFm.Object,
            adminOptions: new AzureAppConfigAdminOptions
            {
                IsAvailable = true,
                Endpoint = "https://example.azconfig.io",
            });

        var flags = await svc.GetFlagsAsync();

        Assert.All(flags, f => Assert.Equal("Azure App Configuration", f.Source));
    }

    [Fact]
    public async Task GetFlagsAsync_PropagatesLabel()
    {
        var mockFm = new Mock<IFeatureManager>();
        mockFm.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);

        var svc = CreateService(
            featureManager: mockFm.Object,
            adminOptions: new AzureAppConfigAdminOptions { IsAvailable = false, Label = "prod" });

        var flags = await svc.GetFlagsAsync();

        Assert.All(flags, f => Assert.Equal("prod", f.Label));
    }

    [Fact]
    public async Task SetFlagAsync_ReturnsFalse_WhenAzureNotAvailable()
    {
        var svc = CreateService(adminOptions: new AzureAppConfigAdminOptions { IsAvailable = false });

        var result = await svc.SetFlagAsync(FeatureFlags.Feature1, enabled: true);

        Assert.False(result);
    }

    [Fact]
    public async Task SetFlagAsync_ReturnsFalse_ForUnknownFlagName()
    {
        var svc = CreateService(adminOptions: new AzureAppConfigAdminOptions
        {
            IsAvailable = true,
            Endpoint = "https://example.azconfig.io",
        });

        var result = await svc.SetFlagAsync("UnknownFlag", enabled: true);

        Assert.False(result);
    }

    [Fact]
    public async Task SetFlagAsync_LogsWarning_WhenAzureNotAvailable()
    {
        var logger = new Mock<ILogger<FeatureFlagService>>();
        var svc = CreateService(
            adminOptions: new AzureAppConfigAdminOptions { IsAvailable = false },
            logger: logger.Object);

        await svc.SetFlagAsync(FeatureFlags.Feature1, enabled: true);

        logger.VerifyLog(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task SetFlagAsync_LogsWarning_ForUnknownFlag()
    {
        var logger = new Mock<ILogger<FeatureFlagService>>();
        var svc = CreateService(
            adminOptions: new AzureAppConfigAdminOptions
            {
                IsAvailable = true,
                Endpoint = "https://example.azconfig.io",
            },
            logger: logger.Object);

        await svc.SetFlagAsync("NotARealFlag", enabled: true);

        logger.VerifyLog(LogLevel.Warning, Times.Once());
    }
}
