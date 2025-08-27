using Mythril.Data;
using Mythril.Data.Items;
using Newtonsoft.Json;

namespace Mythril.Blazor.Services;

public class GameDataService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<Location>> GetLocationsAsync() => await _httpClient.GetFromJsonAsync<List<Location>>("data/locations.json") ?? [];

    public async Task<List<Item>> GetItemsAsync()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = { new ItemConverter() }
        };
        var response = await _httpClient.GetStringAsync("data/items.json");
        return JsonConvert.DeserializeObject<List<Item>>(response, settings) ?? [];
    }

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
