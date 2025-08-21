using Mythril.Data;
using Mythril.Data.Items;
using Mythril.Data.Jobs;
using Mythril.Data.Materia;
using Newtonsoft.Json;

namespace Mythril.Blazor.Services;

public class GameDataService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    private const string _base = "https://localhost:7244/";

    public async Task<List<TaskData>> GetTasksAsync() => await _httpClient.GetFromJsonAsync<List<TaskData>>(_base + "api/tasks") ?? [];

    public async Task<List<Character>> GetCharactersAsync() => await _httpClient.GetFromJsonAsync<List<Character>>(_base + "api/characters") ?? [];

    public async Task<List<Enemy>> GetEnemiesAsync() => await _httpClient.GetFromJsonAsync<List<Enemy>>(_base + "api/enemies") ?? [];

    public async Task<List<Enemy>> GetEnemiesAsync(string zone) => await _httpClient.GetFromJsonAsync<List<Enemy>>(_base + $"api/enemies?zone={zone}") ?? [];

    public async Task<List<Item>> GetItemsAsync()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = { new ItemConverter() }
        };
        var response = await _httpClient.GetStringAsync(_base + "api/items");
        return JsonConvert.DeserializeObject<List<Item>>(response, settings) ?? [];
    }

    public async Task<List<Job>> GetJobsAsync()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = { new JobConverter() }
        };
        var response = await _httpClient.GetStringAsync(_base + "api/jobs");
        return JsonConvert.DeserializeObject<List<Job>>(response, settings) ?? [];
    }

    public async Task<List<Materia>> GetMateriaAsync()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = { new MateriaConverter() }
        };
        var response = await _httpClient.GetStringAsync(_base + "api/materia");
        return JsonConvert.DeserializeObject<List<Materia>>(response, settings) ?? [];
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
