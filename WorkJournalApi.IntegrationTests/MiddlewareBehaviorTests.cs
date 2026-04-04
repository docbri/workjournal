using System.Net;
using System.Text.Json;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class MiddlewareBehaviorTests
{
    [Fact]
    public async Task Unhandled_Exception_Returns_500_With_Json_Response()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/diagnostics/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var content = await response.Content.ReadAsStringAsync();

        Assert.False(string.IsNullOrWhiteSpace(content));

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("Error", out var error));
        Assert.Equal("An unexpected error occurred.", error.GetString());
    }

    [Fact]
    public async Task Normal_Request_Is_Not_Broken_By_Request_Timing_Middleware()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
