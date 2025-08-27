using Mythril.Data;
using Mythril.Data.Items;
using Mythril.Data.Jobs;
using Newtonsoft.Json;

namespace Mythril.Blazor.Services;

public class GameDataService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<Location>> GetLocationsAsync() => await _httpClient.GetFromJsonAsync<List<Location>>("data/tasks.json") ?? [];

    public async Task<List<Item>> GetItemsAsync()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = { new ItemConverter() }
        };
        var response = await _httpClient.GetStringAsync("data/items.json");
        return JsonConvert.DeserializeObject<List<Item>>(response, settings) ?? [];
    }

    public async Task<List<Job>> GetJobsAsync()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = { new JobConverter() }
        };
        var response = await _httpClient.GetStringAsync("data/jobs.json");
        return JsonConvert.DeserializeObject<List<Job>>(response, settings) ?? [];
    }
}

public static class HttpClientExtensions
{
    public static async Task<T?> GetFromJsonAsync<T>(this HttpClient client, string requestUri)
    {
        var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseBody);
    }
}
