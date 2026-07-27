using System.Text.Json.Serialization;

namespace CougarConnect.Models;

/// <summary>
/// A single message sent between two connected alumni, stored in the Supabase "Messages" table.
/// </summary>
public class Message
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    public long SenderId { get; set; }

    public long RecipientId { get; set; }

    public string Body { get; set; } = string.Empty;

    public DateTimeOffset SentAt { get; set; }

    /// <summary>False until the recipient opens the conversation and reads it.</summary>
    public bool IsRead { get; set; }
}
