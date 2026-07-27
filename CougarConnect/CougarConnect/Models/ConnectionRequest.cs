using System.Text.Json.Serialization;

namespace CougarConnect.Models;

/// <summary>
/// A connection request sent from one alumnus to another, stored in the Supabase
/// "ConnectionRequests" table. Status is one of "Pending", "Accepted", "Declined".
/// </summary>
public class ConnectionRequest
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    public long RequesterId { get; set; }

    public long RecipientId { get; set; }

    public string RequestSubject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public DateTimeOffset CreatedAt { get; set; }
}
