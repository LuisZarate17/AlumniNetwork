﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class SupabaseService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://jcbbunghgiboiqzljfyt.supabase.co";
    private const string ApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImpjYmJ1bmdoZ2lib2lxemxqZnl0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzAxNDYzMzUsImV4cCI6MjA0NTcyMjMzNX0.8-yEGxWkQ-c9zn3XSn_4EouHeiUk5qZCSbHkyOIRPUI";

    public SupabaseService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("apikey", ApiKey);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
    }

    public async Task<T[]> GetData<T>(string term,string column)
    {
        var encodedSearch = System.Net.WebUtility.UrlEncode($"ilike.%{term}%");
        var url = $"{BaseUrl}/rest/v1/Alumni?{column}={encodedSearch}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();  // Throw an exception if the response is not successful

        return await response.Content.ReadFromJsonAsync <T[]>();
    }

    public async Task PostData<T>(string tableName, T data)
    {
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/rest/v1/{tableName}", data);
        response.EnsureSuccessStatusCode();
    }
}
