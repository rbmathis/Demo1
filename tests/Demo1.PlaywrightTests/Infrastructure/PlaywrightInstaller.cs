using System;
using System.Threading.Tasks;

namespace Demo1.PlaywrightTests.Infrastructure;

internal static class PlaywrightInstaller
{
    private static readonly object _lock = new();
    private static volatile bool _installed;

    public static async Task EnsureBrowsersAsync()
    {
        if (_installed)
        {
            return;
        }

        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_installed)
                {
                    return;
                }

                // Ensure the Playwright browser binaries are available before tests run.
                var exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });

                if (exitCode != 0)
                {
                    throw new InvalidOperationException($"Playwright browser installation failed with exit code {exitCode}.");
                }

                _installed = true;
            }
        }).ConfigureAwait(false);
    }
}
