using System.Text.Json.Serialization;

namespace CougarConnect.Models;

public class Notification
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
