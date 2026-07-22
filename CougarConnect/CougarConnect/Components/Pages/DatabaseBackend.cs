using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text;

public class SupabaseService
{
    private readonly HttpClient _httpClient;
    private readonly string BaseUrl;

    public SupabaseService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        BaseUrl = configuration["Supabase:Url"];
        var apiKey = configuration["Supabase:ApiKey"];
        _httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
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
    public async void AddConnection(long connectionId, long userId)
    {
        Item[] User;
        var encodedSearch = System.Net.WebUtility.UrlEncode($"ilike.%{userId}%");
        var url = $"{BaseUrl}/rest/v1/Alumni?id=eq.{userId}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();  // Throw an exception if the response is not successful

        User = await response.Content.ReadFromJsonAsync<Item[]>();
        List<long> temp = User[0].ConnectionList.ToList();
        if (temp.Contains(connectionId) != true)
        {
            temp.Add(connectionId);
            User[0].ConnectionList = temp.ToArray();
        }

        await PostData<Item>("Alumni", User[0]);
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
