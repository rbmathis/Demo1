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
    private Process? _process;
    private readonly string _projectDirectory;
    private Uri? _baseAddress;

    public Demo1ServerFixture()
    {
        _projectDirectory = LocateProjectDirectory();
    }

    public Uri BaseAddress => _baseAddress ?? throw new InvalidOperationException("Server has not been started. Call StartAsync first.");

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_process is not null)
        {
            return;
        }

        // Use port 0 to let Kestrel automatically assign a free port
        // This avoids the race condition where another process could grab the port
        // between when we find a free port and when the server starts listening
        var startInfo = new ProcessStartInfo("dotnet", "run --no-build --urls http://127.0.0.1:0")
        {
            WorkingDirectory = _projectDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        _process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start Demo1 application.");

        // Read the actual port from server output and wait for healthy
        await WaitForServerReadyAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task WaitForServerReadyAsync(CancellationToken cancellationToken)
    {
        if (_process is null)
        {
            throw new InvalidOperationException("Process not started.");
        }

        var portDiscoveryTimeout = TimeSpan.FromSeconds(30);
        var portRegex = PortListeningRegex();
        var portDiscovered = false;
        var startTime = DateTime.UtcNow;

        // Read output to discover the dynamically assigned port
        while (!portDiscovered && DateTime.UtcNow - startTime < portDiscoveryTimeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_process.HasExited)
            {
                throw new InvalidOperationException($"Demo1 application exited unexpectedly with code {_process.ExitCode}.");
            }

            var line = await _process.StandardOutput.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
            {
                continue;
            }

            var match = portRegex.Match(line);
            if (match.Success)
            {
                var url = match.Groups[1].Value;
                _baseAddress = new Uri(url);
                portDiscovered = true;
            }
        }

        if (!portDiscovered)
        {
            throw new InvalidOperationException("Failed to discover the server port from output within the expected time.");
        }

        // Now verify the server is healthy
        await WaitForHealthyAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task WaitForHealthyAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient { BaseAddress = _baseAddress };

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
            catch
            {
                // Server not ready yet.
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException("Demo1 application did not start listening within the expected time.");
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"Now listening on:\s*(http://[^\s]+)", RegexOptions.IgnoreCase)]
    private static partial Regex PortListeningRegex();

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
