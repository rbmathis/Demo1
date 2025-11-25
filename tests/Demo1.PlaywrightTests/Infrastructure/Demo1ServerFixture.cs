using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Demo1.PlaywrightTests.Infrastructure;

internal sealed partial class Demo1ServerFixture : IAsyncDisposable
{
    // Pattern to match "Now listening on: http://127.0.0.1:XXXXX"
    [System.Text.RegularExpressions.GeneratedRegex(@"Now listening on:\s*(http://[^\s]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ListeningPattern();

    private Process? _process;
    private readonly string _projectDirectory;

    public Demo1ServerFixture()
    {
        _projectDirectory = LocateProjectDirectory();
    }

    public Uri? BaseAddress { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_process is not null)
        {
            return;
        }

        // Use port 0 to let Kestrel dynamically assign an available port
        // This avoids the race condition of freeing a port before the server starts
        var startInfo = new ProcessStartInfo("dotnet", "run --urls http://127.0.0.1:0")
        {
            WorkingDirectory = _projectDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        _process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start Demo1 application.");

        // Read the actual port from Kestrel's output
        BaseAddress = await ReadBaseAddressFromOutputAsync(_process, cancellationToken).ConfigureAwait(false);

        await WaitForHealthyAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<Uri> ReadBaseAddressFromOutputAsync(Process process, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var timeout = TimeSpan.FromSeconds(60);

        while (DateTime.UtcNow - startTime < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (process.HasExited)
            {
                throw new InvalidOperationException($"Demo1 process exited unexpectedly with code {process.ExitCode}.");
            }

            // Try to read available output
            var line = await process.StandardOutput.ReadLineAsync().ConfigureAwait(false);
            if (line is not null)
            {
                var match = ListeningPattern().Match(line);
                if (match.Success)
                {
                    return new Uri(match.Groups[1].Value);
                }
            }
        }

        throw new InvalidOperationException("Could not determine the listening port from server output within the expected time.");
    }

    private async Task WaitForHealthyAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient { BaseAddress = BaseAddress };

        for (var attempt = 0; attempt < 30; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var response = await client.GetAsync("/", cancellationToken).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Server not ready yet.
            }
            catch (TaskCanceledException)
            {
                // Server not ready yet.
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException("Demo1 application did not start listening within the expected time.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_process is null)
        {
            return;
        }

        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            _process.Dispose();
        }
    }

    private static string LocateProjectDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Demo1.csproj")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new DirectoryNotFoundException($"Unable to locate Demo1 project directory starting from {AppContext.BaseDirectory}.");
        }

        return directory.FullName;
    }
}
