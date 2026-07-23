using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CougarConnect.Models;
using Microsoft.Extensions.Options;

namespace CougarConnect.Services;

public class SupabaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILogger<SupabaseService> _logger;

    public SupabaseService(HttpClient httpClient, IOptions<SupabaseOptions> options, ILogger<SupabaseService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = options.Value.Url;
        var apiKey = options.Value.ApiKey;
        _httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<T[]> GetData<T>(string term, string column)
    {
        var encodedSearch = System.Net.WebUtility.UrlEncode($"ilike.%{term}%");
        var url = $"{_baseUrl}/rest/v1/Alumni?{column}={encodedSearch}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T[]>() ?? Array.Empty<T>();
    }

    public async Task PostData<T>(string tableName, T data)
    {
        var payload = JsonSerializer.Serialize(data);
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/rest/v1/{tableName}")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Prefer", "return-representation");
        request.Headers.Add("Prefer", "resolution=merge-duplicates");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task AddConnection(long connectionId, long userId)
    {
        try
        {
            var url = $"{_baseUrl}/rest/v1/Alumni?id=eq.{userId}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var matchingAlumni = await response.Content.ReadFromJsonAsync<Alumni[]>() ?? Array.Empty<Alumni>();
            var updatedConnections = matchingAlumni[0].ConnectionList?.ToList() ?? new List<long>();
            if (!updatedConnections.Contains(connectionId))
            {
                updatedConnections.Add(connectionId);
                matchingAlumni[0].ConnectionList = updatedConnections.ToArray();
            }

            await PostData("Alumni", matchingAlumni[0]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add connection {ConnectionId} for user {UserId}", connectionId, userId);
            throw;
        }
    }
}
