using System.Text.Json.Serialization;

namespace CougarConnect.Models;

/// <summary>
/// A single alumni profile as stored in the Supabase "Alumni" table.
/// Consolidates what were previously four independent, drifting copies of this
/// shape (SupabaseService, Connection_Search.razor, Connections.razor, Register.razor).
/// </summary>
public class Alumni
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    public string First { get; set; } = string.Empty;

    public string Last { get; set; } = string.Empty;

    public string GradYear { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Major { get; set; } = string.Empty;

    public string? Company { get; set; }

    public long[]? ConnectionList { get; set; }

    public string Email { get; set; } = string.Empty;
}
