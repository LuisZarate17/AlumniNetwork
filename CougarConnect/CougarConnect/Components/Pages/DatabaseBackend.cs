using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text;

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
        _httpClient.DefaultRequestHeaders.Add("Prefer", "return-representation");
        _httpClient.DefaultRequestHeaders.Add("Prefer", "resolution=merge-duplicates");
       var payload= System.Text.Json.JsonSerializer.Serialize(data);
        var content = new StringContent(payload,Encoding.UTF8,"application/json");
        var response = await _httpClient.PostAsync($"{BaseUrl}/rest/v1/Alumni", content);
        response.EnsureSuccessStatusCode();
    }
    public async Task<int> NewID()
    {
        int idnew = 0;
        var url = $"{BaseUrl}/rest/v1/Alumni?id";
        int[] ids;
        var response = await _httpClient.GetAsync(url);
        ids = await response.Content.ReadFromJsonAsync<int[]>();
        idnew = ids.Max();
        idnew++;
        return idnew;
    }
    public async void AddConnection(int connectionId,string userId)
    {
        Item User;
        var encodedSearch = System.Net.WebUtility.UrlEncode($"ilike.%userId%");
        var url = $"{BaseUrl}/rest/v1/Alumni?id={encodedSearch}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();  // Throw an exception if the response is not successful

        User = await response.Content.ReadFromJsonAsync<Item>();
        List<long> temp = User.ConnectionList.ToList();
        temp.Add(connectionId);
        User.ConnectionList = temp.ToArray();
        PostData<Item>("Alumni", User);
    }
    public class Item
    {
        public long id { get; set; }
        public string First { get; set; }
        public string Last { get; set; }
        public string GradYear { get; set; }
        public string City { get; set; }
        public string Major { get; set; }
        public string Company { get; set; }
        public long[] ConnectionList { get; set; }
        public string Email { get; set; }

    }
}
