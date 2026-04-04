using System.Net;
using System.Text.Json;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class WorkItemReadTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public WorkItemReadTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_All_WorkItems_Returns_Success_And_Json_Array()
    {
        const string route = "/work-items";

        var response = await _client.GetAsync(route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);

        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
