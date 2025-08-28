using Mythril.Data;
using Newtonsoft.Json;

namespace Mythril.Blazor.Services;

public class GameDataService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<Location>> GetLocationsAsync() => await _httpClient.GetFromJsonAsync<List<Location>>("data/locations.json") ?? [];

    public async Task<List<Item>> GetItemsAsync() => await _httpClient.GetFromJsonAsync<List<Item>>("data/items.json") ?? [];

    public async Task<List<Cadence>> GetCadencesAsync() => await _httpClient.GetFromJsonAsync<List<Cadence>>("data/cadences.json") ?? [];
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
