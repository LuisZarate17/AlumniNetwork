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
}
