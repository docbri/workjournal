using System.Net;
using System.Text.Json;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class ApplicationSmokeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApplicationSmokeTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_OpenApi_Document_Returns_Success()
    {
        // Act
        var response = await _client.GetAsync("/openapi/v1.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content));

        using var document = JsonDocument.Parse(content);

        Assert.True(document.RootElement.TryGetProperty("openapi", out var openApiProperty));
        Assert.False(string.IsNullOrWhiteSpace(openApiProperty.GetString()));

        Assert.True(document.RootElement.TryGetProperty("info", out var infoProperty));
        Assert.True(infoProperty.TryGetProperty("title", out var titleProperty));
        Assert.False(string.IsNullOrWhiteSpace(titleProperty.GetString()));
    }

    [Fact]
    public async Task Get_All_WorkItems_Returns_Success_And_Json_Array()
    {
        const string route = "/work-items";

        // Act
        var response = await _client.GetAsync(route);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content));

        using var document = JsonDocument.Parse(content);

        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
    }
}
