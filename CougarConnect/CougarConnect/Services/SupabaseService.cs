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

    private static readonly string[] AlumniSearchColumns =
        { "First", "Last", "Email", "GradYear", "City", "Major", "Company" };

    public async Task<T[]> GetData<T>(string term, string column)
    {
        var encodedSearch = System.Net.WebUtility.UrlEncode($"ilike.%{term}%");
        var url = $"{_baseUrl}/rest/v1/Alumni?{column}={encodedSearch}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T[]>() ?? Array.Empty<T>();
    }

    /// <summary>
    /// Looks up a single alumnus by their numeric primary key. Uses an exact <c>eq</c>
    /// filter rather than <see cref="GetData{T}"/>, whose <c>ilike</c> (text pattern) filter
    /// is invalid against the <c>bigint</c> id column and makes PostgREST return an error.
    /// </summary>
    public async Task<Alumni?> GetAlumniById(long id)
    {
        var url = $"{_baseUrl}/rest/v1/Alumni?id=eq.{id}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<Alumni[]>() ?? Array.Empty<Alumni>();
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Searches every alumni text column for the term at once (PostgREST "or" filter),
    /// so a single search box matches a name, email, city, major, etc. without the
    /// caller having to pick a column.
    /// </summary>
    public async Task<Alumni[]> SearchAlumni(string term)
    {
        var escapedTerm = Uri.EscapeDataString(term);
        var orExpression = string.Join(",", AlumniSearchColumns.Select(c => $"{c}.ilike.*{escapedTerm}*"));
        var url = $"{_baseUrl}/rest/v1/Alumni?or=({orExpression})";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Alumni[]>() ?? Array.Empty<Alumni>();
    }

    /// <summary>
    /// Returns every alumnus in the directory. Used to build "people you may know"
    /// recommendations, which are then filtered client-side (excluding self and
    /// existing connections).
    /// </summary>
    public async Task<Alumni[]> GetAllAlumni()
    {
        var url = $"{_baseUrl}/rest/v1/Alumni";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Alumni[]>() ?? Array.Empty<Alumni>();
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

    /// <summary>
    /// Returns the messages sent to this user that they haven't read yet, used to show
    /// unread indicators (e.g. on the Connections page).
    /// </summary>
    public async Task<Message[]> GetUnreadMessages(long recipientId)
    {
        var url = $"{_baseUrl}/rest/v1/Messages?RecipientId=eq.{recipientId}&IsRead=eq.false";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Message[]>() ?? Array.Empty<Message>();
    }

    public async Task<List<Message>> GetConversation(long userId, long otherUserId)
    {
        var sent = await GetMessagesBetween(userId, otherUserId);
        var received = await GetMessagesBetween(otherUserId, userId);
        return sent.Concat(received).OrderBy(m => m.SentAt).ToList();
    }

    private async Task<Message[]> GetMessagesBetween(long senderId, long recipientId)
    {
        var url = $"{_baseUrl}/rest/v1/Messages?SenderId=eq.{senderId}&RecipientId=eq.{recipientId}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Message[]>() ?? Array.Empty<Message>();
    }

    public async Task<ConnectionRequest?> GetConnectionRequestByToken(string token)
    {
        var url = $"{_baseUrl}/rest/v1/ConnectionRequests?Token=eq.{Uri.EscapeDataString(token)}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<ConnectionRequest[]>() ?? Array.Empty<ConnectionRequest>();
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Returns the pending connection requests sent TO this user, so they can accept or
    /// decline them in-app (e.g. from the Recent Activity page) without the emailed link.
    /// </summary>
    public async Task<ConnectionRequest[]> GetIncomingPendingRequests(long recipientId)
    {
        var url = $"{_baseUrl}/rest/v1/ConnectionRequests?RecipientId=eq.{recipientId}&Status=eq.Pending";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<ConnectionRequest[]>() ?? Array.Empty<ConnectionRequest>();
        return results.OrderByDescending(r => r.CreatedAt).ToArray();
    }

    public async Task<bool> ConnectionRequestExists(long requesterId, long recipientId)
    {
        var url = $"{_baseUrl}/rest/v1/ConnectionRequests?RequesterId=eq.{requesterId}&RecipientId=eq.{recipientId}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<ConnectionRequest[]>() ?? Array.Empty<ConnectionRequest>();
        return results.Length > 0;
    }

    public async Task AddMutualConnection(long userAId, long userBId)
    {
        await AddConnection(userBId, userAId);
        await AddConnection(userAId, userBId);
    }

    public async Task<List<Notification>> GetNotifications(long userId)
    {
        var url = $"{_baseUrl}/rest/v1/Notifications?UserId=eq.{userId}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<Notification[]>() ?? Array.Empty<Notification>();
        return results.OrderByDescending(n => n.CreatedAt).ToList();
    }

    public async Task AddNotification(long userId, string message)
    {
        var notification = new Notification
        {
            Id = Random.Shared.NextInt64(1, long.MaxValue),
            UserId = userId,
            Message = message,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await PostData("Notifications", notification);
    }
}
