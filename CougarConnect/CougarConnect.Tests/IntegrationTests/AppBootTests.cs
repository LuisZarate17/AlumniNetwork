using System.Text.RegularExpressions;
using CougarConnect.Tests;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CougarConnect.Tests.IntegrationTests;

public class AppBootTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly Regex HiddenInputRegex = new(
        "<input[^>]*type=\"hidden\"[^>]*/?>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex NameAttributeRegex = new(
        "name=\"([^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ValueAttributeRegex = new(
        "value=\"([^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly CustomWebApplicationFactory _factory;

    public AppBootTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHome_ReturnsSuccess()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task GetLoginPage_ReturnsSuccess()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Account/Login");

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task PostLogin_WithValidSeededUser_RedirectsAfterSignIn()
    {
        const string email = "test-user@example.test";
        const string password = "P@ssw0rd!123";
        await _factory.SeedTestUserAsync(email, password);

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var loginPageHtml = await (await client.GetAsync("/Account/Login")).Content.ReadAsStringAsync();

        var form = ExtractHiddenFields(loginPageHtml);
        form["Input.Email"] = email;
        form["Input.Password"] = password;

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(form));

        Assert.True((int)response.StatusCode is >= 300 and < 400,
            $"Expected a redirect after successful sign-in, got {(int)response.StatusCode}.");
    }

    private static Dictionary<string, string> ExtractHiddenFields(string html)
    {
        var fields = new Dictionary<string, string>();
        foreach (Match inputMatch in HiddenInputRegex.Matches(html))
        {
            var tag = inputMatch.Value;
            var nameMatch = NameAttributeRegex.Match(tag);
            if (!nameMatch.Success)
            {
                continue;
            }

            var valueMatch = ValueAttributeRegex.Match(tag);
            fields[nameMatch.Groups[1].Value] = valueMatch.Success ? valueMatch.Groups[1].Value : string.Empty;
        }

        return fields;
    }
}
