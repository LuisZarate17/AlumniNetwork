using System.Text.Json;
using CougarConnect.Models;

namespace CougarConnect.Tests.Models;

public class AlumniModelTests
{
    [Fact]
    public void Alumni_SerializesIdAsLowercaseId()
    {
        var alumni = new Alumni { Id = 42, First = "Ada", Last = "Lovelace", Email = "ada@example.com" };

        var json = JsonSerializer.Serialize(alumni);

        Assert.Contains("\"id\":42", json);
        Assert.DoesNotContain("\"Id\":", json);
    }

    [Fact]
    public void Alumni_DeserializesLowercaseIdIntoIdProperty()
    {
        var json = "{\"id\":42,\"First\":\"Ada\",\"Last\":\"Lovelace\",\"Email\":\"ada@example.com\"}";

        var alumni = JsonSerializer.Deserialize<Alumni>(json);

        Assert.NotNull(alumni);
        Assert.Equal(42, alumni!.Id);
    }

    [Fact]
    public void Alumni_RoundTripsThroughSerializeDeserialize()
    {
        var original = new Alumni
        {
            Id = 7,
            First = "Grace",
            Last = "Hopper",
            GradYear = "1928",
            City = "New York",
            Major = "Mathematics",
            Company = "Navy",
            ConnectionList = new long[] { 1, 2, 3 },
            Email = "grace@example.com"
        };

        var json = JsonSerializer.Serialize(original);
        var roundTripped = JsonSerializer.Deserialize<Alumni>(json);

        Assert.NotNull(roundTripped);
        Assert.Equal(original.Id, roundTripped!.Id);
        Assert.Equal(original.First, roundTripped.First);
        Assert.Equal(original.Last, roundTripped.Last);
        Assert.Equal(original.Email, roundTripped.Email);
        Assert.Equal(original.ConnectionList, roundTripped.ConnectionList);
    }
}
