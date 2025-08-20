using Mythril.Data;
using Mythril.Data.Items;
using Mythril.Data.Jobs;
using Mythril.Data.Materia;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Mythril.Blazor.Services;

public class GameDataService
{
    private readonly HttpClient _httpClient;

    public GameDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CardData>> GetCardsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<CardData>>("api/cards");
    }

    public async Task<List<Character>> GetCharactersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Character>>("api/characters");
    }

    public async Task<List<Enemy>> GetEnemiesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Enemy>>("api/enemies");
    }

    public async Task<List<Item>> GetItemsAsync()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = { new ItemConverter() }
        };
        var response = await _httpClient.GetStringAsync("api/items");
        return JsonConvert.DeserializeObject<List<Item>>(response, settings);
    }

    public async Task<List<Job>> GetJobsAsync()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = { new JobConverter() }
        };
        var response = await _httpClient.GetStringAsync("api/jobs");
        return JsonConvert.DeserializeObject<List<Job>>(response, settings);
    }

    public async Task<List<Materia>> GetMateriaAsync()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = { new MateriaConverter() }
        };
        var response = await _httpClient.GetStringAsync("api/materia");
        return JsonConvert.DeserializeObject<List<Materia>>(response, settings);
    }
}

public static class HttpClientExtensions
{
    public static async Task<T> GetFromJsonAsync<T>(this HttpClient client, string requestUri)
    {
        var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseBody);
    }
}
