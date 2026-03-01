using Microsoft.JSInterop;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace Mythril.Blazor.Services;

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
}

public class FeedbackService(IJSRuntime js, IConfiguration config, HttpClient http)
{
    private const string STORAGE_KEY = "mythril_pending_feedback";
    private readonly string _remoteUrl = config["Feedback:WebAppUrl"] ?? "";

    public async Task AddFeedback(FeedbackEntry entry)
    {
        var all = await GetPendingFeedback();
        all.Add(entry);
        await SaveAll(all);
        
        // Background attempt to submit to remote
        _ = Task.Run(() => SubmitToRemote(entry));
    }

    private async Task SubmitToRemote(FeedbackEntry entry)
    {
        if (string.IsNullOrEmpty(_remoteUrl) || _remoteUrl == "PLACEHOLDER_URL") return;

        try
        {
            // Simple POST to Google Apps Script
            await http.PostAsJsonAsync(_remoteUrl, entry);
        }
        catch
        {
            // Fail silently, it's still saved locally
        }
    }

    public async Task CaptureError(string message, string? stackTrace = null)
    {
        await AddFeedback(new FeedbackEntry
        {
            Type = FeedbackType.Error,
            Title = "Automated Error Report",
            Description = message,
            StackTrace = stackTrace
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
