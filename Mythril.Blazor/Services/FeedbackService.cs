using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace Mythril.Blazor.Services;

[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum FeedbackType
{
    Bug,
    FeatureRequest,
    Suggestion,
    Error
}

public class FeedbackEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public FeedbackType Type { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Source { get; set; } = "In-Game UI";
    public string? StackTrace { get; set; }
    public string? ConsoleLog { get; set; }
}

public class FeedbackService(IJSRuntime js, AuthService auth, HttpClient http)
{
    private const string STORAGE_KEY = "mythril_pending_feedback";

    public async Task AddFeedback(FeedbackEntry entry)
    {
        var all = await GetPendingFeedback();
        all.Add(entry);
        await SaveAll(all);

        if (auth.IsAuthenticated)
        {
            // Auto-sync to local dev bridge if it exists
            _ = Task.Run(() => SyncToDevBridge(entry));
        }
    }

    private async Task SyncToDevBridge(FeedbackEntry entry)
    {
        try
        {
            Console.WriteLine($"[FeedbackService] Syncing {entry.Type} to bridge...");
            
            // Use custom options to ensure Enum is serialized as string
            var options = new System.Text.Json.JsonSerializerOptions
            {
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };

            // The dev bridge runs on a fixed local port during development
            await http.PostAsJsonAsync("http://localhost:8080/report", entry, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FeedbackService] Bridge sync failed: {ex.Message}");
        }
    }

    public async Task CaptureError(string message, string? stackTrace = null)
    {
        string? logs = null;
        try 
        {
            logs = await js.InvokeAsync<string>("window.getRecentLogs");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get logs: {ex.Message}");
        }

        await AddFeedback(new FeedbackEntry
        {
            Type = FeedbackType.Error,
            Title = "Automated Error Report",
            Description = message,
            StackTrace = stackTrace,
            ConsoleLog = logs
        });
    }

    public async Task<List<FeedbackEntry>> GetPendingFeedback()
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);
        if (string.IsNullOrEmpty(json)) return [];
        return JsonConvert.DeserializeObject<List<FeedbackEntry>>(json) ?? [];
    }

    private async Task SaveAll(List<FeedbackEntry> entries)
    {
        var json = JsonConvert.SerializeObject(entries);
        await js.InvokeVoidAsync("localStorage.setItem", STORAGE_KEY, json);
    }

    public async Task ClearFeedback()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", STORAGE_KEY);
    }

    public string GetGitHubIssueUrl(FeedbackEntry entry)
    {
        var baseUrl = "https://github.com/DerekGooding/Mythril/issues/new";
        var title = Uri.EscapeDataString($"[{entry.Type}] {entry.Title}");
        var body = Uri.EscapeDataString($"""
**Date:** {entry.Date}
**Type:** {entry.Type}
**Source:** {entry.Source}

## Description
{entry.Description}

## Stack Trace
```
{entry.StackTrace}
```
""");
        return $"{baseUrl}?title={title}&body={body}";
    }
}
