using System.Text.Json;
using CougarConnect.Models;

namespace CougarConnect.Tests.Models;

public class ConnectionRequestModelTests
{
    [Fact]
    public void ConnectionRequest_SerializesIdAsLowercaseId()
    {
        var request = new ConnectionRequest { Id = 42, RequesterId = 1, RecipientId = 2 };

        var json = JsonSerializer.Serialize(request);

        Assert.Contains("\"id\":42", json);
        Assert.DoesNotContain("\"Id\":", json);
    }

    [Fact]
    public void ConnectionRequest_DeserializesLowercaseIdIntoIdProperty()
    {
        var json = "{\"id\":42,\"RequesterId\":1,\"RecipientId\":2,\"RequestSubject\":\"Hi\",\"Message\":\"Hello\",\"Token\":\"abc123\",\"Status\":\"Pending\",\"CreatedAt\":\"2026-01-01T00:00:00+00:00\"}";

        var request = JsonSerializer.Deserialize<ConnectionRequest>(json);

        Assert.NotNull(request);
        Assert.Equal(42, request!.Id);
    }

    [Fact]
    public void ConnectionRequest_RoundTripsThroughSerializeDeserialize()
    {
        var original = new ConnectionRequest
        {
            Id = 7,
            RequesterId = 1,
            RecipientId = 2,
            RequestSubject = "Let's connect",
            Message = "Great to meet a fellow alum!",
            Token = "abc123token",
            Status = "Pending",
            CreatedAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)
        };

        var json = JsonSerializer.Serialize(original);
        var roundTripped = JsonSerializer.Deserialize<ConnectionRequest>(json);

        Assert.NotNull(roundTripped);
        Assert.Equal(original.Id, roundTripped!.Id);
        Assert.Equal(original.RequesterId, roundTripped.RequesterId);
        Assert.Equal(original.RecipientId, roundTripped.RecipientId);
        Assert.Equal(original.RequestSubject, roundTripped.RequestSubject);
        Assert.Equal(original.Message, roundTripped.Message);
        Assert.Equal(original.Token, roundTripped.Token);
        Assert.Equal(original.Status, roundTripped.Status);
        Assert.Equal(original.CreatedAt, roundTripped.CreatedAt);
    }

    [Fact]
    public void ConnectionRequest_DefaultsStatusToPending()
    {
        var request = new ConnectionRequest();

        Assert.Equal("Pending", request.Status);
    }
}
