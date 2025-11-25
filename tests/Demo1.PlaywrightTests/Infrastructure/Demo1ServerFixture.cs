using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Demo1.PlaywrightTests.Infrastructure;

internal sealed class Demo1ServerFixture : IAsyncDisposable
{
    private Process? _process;
    private readonly int _port;
    private readonly string _projectDirectory;

    public Demo1ServerFixture()
    {
        _port = GetFreeTcpPort();
        _projectDirectory = LocateProjectDirectory();
        BaseAddress = new Uri($"http://127.0.0.1:{_port}");
    }

    public Uri BaseAddress { get; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_process is not null)
        {
            return;
        }

        var startInfo = new ProcessStartInfo("dotnet", $"run --urls {BaseAddress}")
        {
            WorkingDirectory = _projectDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        _process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start Demo1 application.");

        await WaitForHealthyAsync(cancellationToken).ConfigureAwait(false);
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

    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
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
