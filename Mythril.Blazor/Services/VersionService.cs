using System.Net.Http.Json;

namespace Mythril.Blazor.Services;

public class VersionInfo
{
    public string Version { get; set; } = "";
    public DateTime Timestamp { get; set; }
}

public class VersionService(HttpClient http) : IDisposable
{
    private readonly System.Timers.Timer _timer = new(TimeSpan.FromMinutes(5).TotalMilliseconds);
    public string? CurrentVersion { get; private set; }
    public bool IsUpdateAvailable { get; private set; } = false;

    public event Action? OnUpdateAvailable;

    public void Initialize()
    {
        _timer.Elapsed += async (s, e) => await CheckVersion();
        _timer.AutoReset = true;
        _timer.Start();

        // Initial check
        _ = Task.Run(CheckVersion);
    }

    private async Task CheckVersion()
    {
        try
        {
            // Use cache buster to ensure we get the latest file
            var info = await http.GetFromJsonAsync<VersionInfo>($"version.json?t={DateTime.UtcNow.Ticks}");
            if (info != null)
            {
                Console.WriteLine($"Version Check: Local={CurrentVersion ?? "null"}, Remote={info.Version}");
                if (CurrentVersion == null)
                {
                    CurrentVersion = info.Version;
                }
                else if (CurrentVersion != info.Version)
                {
                    Console.WriteLine("New version detected! Triggering update notification.");
                    IsUpdateAvailable = true;
                    OnUpdateAvailable?.Invoke();
                    _timer.Stop(); // Only notify once
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Version Check Failed: {ex.Message}");
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _timer.Dispose();
    }
}