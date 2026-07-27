using System.Text.Json;
using CougarConnect.Models;

namespace CougarConnect.Tests.Models;

public class MessageModelTests
{
    [Fact]
    public void Message_SerializesIdAsLowercaseId()
    {
        var message = new Message { Id = 42, SenderId = 1, RecipientId = 2, Body = "Hello" };

        var json = JsonSerializer.Serialize(message);

        Assert.Contains("\"id\":42", json);
        Assert.DoesNotContain("\"Id\":", json);
    }

    [Fact]
    public void Message_DeserializesLowercaseIdIntoIdProperty()
    {
        var json = "{\"id\":42,\"SenderId\":1,\"RecipientId\":2,\"Body\":\"Hello\",\"SentAt\":\"2026-01-01T00:00:00+00:00\"}";

        var message = JsonSerializer.Deserialize<Message>(json);

        Assert.NotNull(message);
        Assert.Equal(42, message!.Id);
    }

    [Fact]
    public void Message_RoundTripsThroughSerializeDeserialize()
    {
        var original = new Message
        {
            Id = 7,
            SenderId = 1,
            RecipientId = 2,
            Body = "Great to connect!",
            SentAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            IsRead = true
        };

        var json = JsonSerializer.Serialize(original);
        var roundTripped = JsonSerializer.Deserialize<Message>(json);

        Assert.NotNull(roundTripped);
        Assert.Equal(original.Id, roundTripped!.Id);
        Assert.Equal(original.SenderId, roundTripped.SenderId);
        Assert.Equal(original.RecipientId, roundTripped.RecipientId);
        Assert.Equal(original.Body, roundTripped.Body);
        Assert.Equal(original.SentAt, roundTripped.SentAt);
        Assert.Equal(original.IsRead, roundTripped.IsRead);
    }
}
