using System.Net;
using System.Net.Http.Json;
using CougarConnect.Models;
using CougarConnect.Services;
using CougarConnect.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CougarConnect.Tests.Services;

public class SupabaseServiceTests
{
    private const string BaseUrl = "https://example.test";

    private static SupabaseService CreateService(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new SupabaseOptions { Url = BaseUrl, ApiKey = "test-key" });
        return new SupabaseService(httpClient, options, NullLogger<SupabaseService>.Instance);
    }

    private static HttpResponseMessage JsonResponse<T>(T value) =>
        new(HttpStatusCode.OK) { Content = JsonContent.Create(value) };

    [Fact]
    public async Task GetData_BuildsUrlEncodedIlikeQuery_ForGivenColumn()
    {
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(Array.Empty<Alumni>()));
        var service = CreateService(handler);

        await service.GetData<Alumni>("Smith", "Last");

        var expectedEncodedSearch = WebUtility.UrlEncode("ilike.%Smith%");
        var expectedUrl = $"{BaseUrl}/rest/v1/Alumni?Last={expectedEncodedSearch}";
        Assert.Single(handler.Requests);
        Assert.Equal(expectedUrl, handler.Requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task SearchAlumni_BuildsOrFilterUrl_AcrossAllColumns()
    {
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(Array.Empty<Alumni>()));
        var service = CreateService(handler);

        await service.SearchAlumni("luis");

        var url = handler.Requests[0].RequestUri!.ToString();
        Assert.Contains("or=", url);
        Assert.Contains("First.ilike.", url);
        Assert.Contains("Last.ilike.", url);
        Assert.Contains("Email.ilike.", url);
        Assert.Contains("Company.ilike.", url);
        Assert.Contains("luis", url);
    }

    [Fact]
    public async Task AddConnection_NewConnection_AppendsIdAndPosts()
    {
        var existingAlumni = new Alumni { Id = 1, ConnectionList = new long[] { 10, 20 } };
        var handler = new FakeHttpMessageHandler(req => req.Method == HttpMethod.Get
            ? JsonResponse(new[] { existingAlumni })
            : new HttpResponseMessage(HttpStatusCode.OK));
        var service = CreateService(handler);

        await service.AddConnection(connectionId: 30, userId: 1);

        var postRequest = Assert.Single(handler.Requests, r => r.Method == HttpMethod.Post);
        var postedBody = await postRequest.Content!.ReadFromJsonAsync<Alumni>();
        Assert.Equal(new long[] { 10, 20, 30 }, postedBody!.ConnectionList);
    }

    [Fact]
    public async Task AddConnection_AlreadyConnected_DoesNotDuplicate()
    {
        var existingAlumni = new Alumni { Id = 1, ConnectionList = new long[] { 10, 20 } };
        var handler = new FakeHttpMessageHandler(req => req.Method == HttpMethod.Get
            ? JsonResponse(new[] { existingAlumni })
            : new HttpResponseMessage(HttpStatusCode.OK));
        var service = CreateService(handler);

        await service.AddConnection(connectionId: 20, userId: 1);

        var postRequest = Assert.Single(handler.Requests, r => r.Method == HttpMethod.Post);
        var postedBody = await postRequest.Content!.ReadFromJsonAsync<Alumni>();
        Assert.Equal(new long[] { 10, 20 }, postedBody!.ConnectionList);
    }

    [Fact]
    public async Task PostData_SecondCall_DoesNotLeakPreferHeadersFromFirstCall()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = CreateService(handler);
        var alumni = new Alumni { Id = 1 };

        await service.PostData("Alumni", alumni);
        await service.PostData("Alumni", alumni);

        Assert.Equal(2, handler.Requests.Count);
        var firstPreferValues = handler.Requests[0].Headers.GetValues("Prefer").ToArray();
        var secondPreferValues = handler.Requests[1].Headers.GetValues("Prefer").ToArray();
        Assert.Equal(new[] { "return-representation", "resolution=merge-duplicates" }, firstPreferValues);
        Assert.Equal(firstPreferValues, secondPreferValues);
    }

    [Fact]
    public async Task GetConversation_IssuesTwoEqFilteredRequests_AndReturnsMergedSortedBySentAt()
    {
        var laterMessage = new Message { Id = 1, SenderId = 1, RecipientId = 2, Body = "Hi from A", SentAt = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero) };
        var earlierMessage = new Message { Id = 2, SenderId = 2, RecipientId = 1, Body = "Hi from B", SentAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) };

        var handler = new FakeHttpMessageHandler(req =>
        {
            var query = req.RequestUri!.Query;
            return query.Contains("SenderId=eq.1") && query.Contains("RecipientId=eq.2")
                ? JsonResponse(new[] { laterMessage })
                : JsonResponse(new[] { earlierMessage });
        });
        var service = CreateService(handler);

        var conversation = await service.GetConversation(userId: 1, otherUserId: 2);

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal($"{BaseUrl}/rest/v1/Messages?SenderId=eq.1&RecipientId=eq.2", handler.Requests[0].RequestUri!.ToString());
        Assert.Equal($"{BaseUrl}/rest/v1/Messages?SenderId=eq.2&RecipientId=eq.1", handler.Requests[1].RequestUri!.ToString());
        Assert.Equal(new[] { earlierMessage.Id, laterMessage.Id }, conversation.Select(m => m.Id));
    }

    [Fact]
    public async Task GetAllAlumni_RequestsUnfilteredAlumniEndpoint()
    {
        var alumni = new[] { new Alumni { Id = 1 }, new Alumni { Id = 2 } };
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(alumni));
        var service = CreateService(handler);

        var result = await service.GetAllAlumni();

        Assert.Equal($"{BaseUrl}/rest/v1/Alumni", handler.Requests[0].RequestUri!.ToString());
        Assert.Equal(2, result.Length);
    }

    [Fact]
    public async Task GetUnreadMessages_BuildsRecipientAndUnreadFilteredUrl()
    {
        var unread = new Message { Id = 1, SenderId = 5, RecipientId = 9, Body = "Hi", IsRead = false };
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(new[] { unread }));
        var service = CreateService(handler);

        var result = await service.GetUnreadMessages(9);

        var expectedUrl = $"{BaseUrl}/rest/v1/Messages?RecipientId=eq.9&IsRead=eq.false";
        Assert.Equal(expectedUrl, handler.Requests[0].RequestUri!.ToString());
        Assert.Single(result);
    }

    [Fact]
    public async Task GetConnectionRequestByToken_BuildsEqFilteredUrl_AndReturnsFirstMatch()
    {
        var matching = new ConnectionRequest { Id = 5, RequesterId = 1, RecipientId = 2, Token = "abc123", Status = "Pending" };
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(new[] { matching }));
        var service = CreateService(handler);

        var result = await service.GetConnectionRequestByToken("abc123");

        var expectedUrl = $"{BaseUrl}/rest/v1/ConnectionRequests?Token=eq.abc123";
        Assert.Single(handler.Requests);
        Assert.Equal(expectedUrl, handler.Requests[0].RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Equal(5, result!.Id);
    }

    [Fact]
    public async Task GetConnectionRequestByToken_NoMatch_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(Array.Empty<ConnectionRequest>()));
        var service = CreateService(handler);

        var result = await service.GetConnectionRequestByToken("missing-token");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetIncomingPendingRequests_BuildsRecipientAndStatusFilteredUrl()
    {
        var pending = new ConnectionRequest { Id = 9, RequesterId = 3, RecipientId = 7, Status = "Pending" };
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(new[] { pending }));
        var service = CreateService(handler);

        var result = await service.GetIncomingPendingRequests(7);

        var expectedUrl = $"{BaseUrl}/rest/v1/ConnectionRequests?RecipientId=eq.7&Status=eq.Pending";
        Assert.Equal(expectedUrl, handler.Requests[0].RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(9, result[0].Id);
    }

    [Fact]
    public async Task ConnectionRequestExists_MatchFound_ReturnsTrue()
    {
        var existing = new ConnectionRequest { Id = 1, RequesterId = 1, RecipientId = 2, Status = "Pending" };
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(new[] { existing }));
        var service = CreateService(handler);

        var exists = await service.ConnectionRequestExists(1, 2);

        var expectedUrl = $"{BaseUrl}/rest/v1/ConnectionRequests?RequesterId=eq.1&RecipientId=eq.2";
        Assert.Equal(expectedUrl, handler.Requests[0].RequestUri!.ToString());
        Assert.True(exists);
    }

    [Fact]
    public async Task ConnectionRequestExists_NoMatch_ReturnsFalse()
    {
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(Array.Empty<ConnectionRequest>()));
        var service = CreateService(handler);

        var exists = await service.ConnectionRequestExists(1, 2);

        Assert.False(exists);
    }

    [Fact]
    public async Task AddMutualConnection_AddsEachUserToTheOthersConnectionList()
    {
        var userA = new Alumni { Id = 1, ConnectionList = Array.Empty<long>() };
        var userB = new Alumni { Id = 2, ConnectionList = Array.Empty<long>() };

        var handler = new FakeHttpMessageHandler(req =>
        {
            if (req.Method == HttpMethod.Get)
            {
                var query = req.RequestUri!.Query;
                return query.Contains("id=eq.1")
                    ? JsonResponse(new[] { userA })
                    : JsonResponse(new[] { userB });
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var service = CreateService(handler);

        await service.AddMutualConnection(userAId: 1, userBId: 2);

        var getRequests = handler.Requests.Where(r => r.Method == HttpMethod.Get).ToList();
        var postRequests = handler.Requests.Where(r => r.Method == HttpMethod.Post).ToList();
        Assert.Equal(2, getRequests.Count);
        Assert.Equal(2, postRequests.Count);

        var postedBodies = new List<Alumni>();
        foreach (var post in postRequests)
        {
            postedBodies.Add((await post.Content!.ReadFromJsonAsync<Alumni>())!);
        }

        var postedForA = postedBodies.Single(a => a.Id == 1);
        var postedForB = postedBodies.Single(a => a.Id == 2);
        Assert.Contains(2L, postedForA.ConnectionList!);
        Assert.Contains(1L, postedForB.ConnectionList!);
    }

    [Fact]
    public async Task GetNotifications_BuildsEqFilteredUrl_AndReturnsNewestFirst()
    {
        var older = new Notification { Id = 1, UserId = 1, Message = "Older", CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) };
        var newer = new Notification { Id = 2, UserId = 1, Message = "Newer", CreatedAt = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero) };
        var handler = new FakeHttpMessageHandler(_ => JsonResponse(new[] { older, newer }));
        var service = CreateService(handler);

        var result = await service.GetNotifications(1);

        var expectedUrl = $"{BaseUrl}/rest/v1/Notifications?UserId=eq.1";
        Assert.Single(handler.Requests);
        Assert.Equal(expectedUrl, handler.Requests[0].RequestUri!.ToString());
        Assert.Equal(new[] { newer.Id, older.Id }, result.Select(n => n.Id));
    }

    [Fact]
    public async Task AddNotification_PostsMessageAndUserId()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = CreateService(handler);

        await service.AddNotification(5, "Jordan Rivera accepted your connection request.");

        var postRequest = Assert.Single(handler.Requests, r => r.Method == HttpMethod.Post);
        Assert.Equal($"{BaseUrl}/rest/v1/Notifications", postRequest.RequestUri!.ToString());
        var postedBody = await postRequest.Content!.ReadFromJsonAsync<Notification>();
        Assert.Equal(5, postedBody!.UserId);
        Assert.Equal("Jordan Rivera accepted your connection request.", postedBody.Message);
    }
}
