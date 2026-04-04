using System.Net;
using System.Net.Http.Json;
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

        var document = await response.Content.ReadFromJsonAsync<OpenApiDocumentResponse>();

        Assert.NotNull(document);
        Assert.False(string.IsNullOrWhiteSpace(document!.OpenApi));
        Assert.False(string.IsNullOrWhiteSpace(document.Info.Title));
    }

    public sealed class OpenApiDocumentResponse
    {
        public string OpenApi { get; set; } = string.Empty;

        public InfoResponse Info { get; set; } = new();
    }

    public sealed class InfoResponse
    {
        public string Title { get; set; } = string.Empty;
    }
}
