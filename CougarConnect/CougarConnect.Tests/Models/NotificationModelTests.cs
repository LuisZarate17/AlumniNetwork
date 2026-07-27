using System.Text.Json;
using CougarConnect.Models;

namespace CougarConnect.Tests.Models;

public class NotificationModelTests
{
    [Fact]
    public void Notification_SerializesIdAsLowercaseId()
    {
        var notification = new Notification { Id = 42, UserId = 1, Message = "Hello" };

        var json = JsonSerializer.Serialize(notification);

        Assert.Contains("\"id\":42", json);
        Assert.DoesNotContain("\"Id\":", json);
    }

    [Fact]
    public void Notification_DeserializesLowercaseIdIntoIdProperty()
    {
        var json = "{\"id\":42,\"UserId\":1,\"Message\":\"Hello\",\"CreatedAt\":\"2026-01-01T00:00:00+00:00\"}";

        var notification = JsonSerializer.Deserialize<Notification>(json);

        Assert.NotNull(notification);
        Assert.Equal(42, notification!.Id);
    }

    [Fact]
    public void Notification_RoundTripsThroughSerializeDeserialize()
    {
        var original = new Notification
        {
            Id = 7,
            UserId = 1,
            Message = "Jordan Rivera accepted your connection request.",
            CreatedAt = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)
        };

        var json = JsonSerializer.Serialize(original);
        var roundTripped = JsonSerializer.Deserialize<Notification>(json);

        Assert.NotNull(roundTripped);
        Assert.Equal(original.Id, roundTripped!.Id);
        Assert.Equal(original.UserId, roundTripped.UserId);
        Assert.Equal(original.Message, roundTripped.Message);
        Assert.Equal(original.CreatedAt, roundTripped.CreatedAt);
    }
}
