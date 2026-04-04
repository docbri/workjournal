using System.Net;
using System.Text.Json;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class WorkItemDiagnosticsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public WorkItemDiagnosticsTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_Health_Returns_Healthy_Status()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("status", out var status));
        Assert.Equal("Healthy", status.GetString());

        Assert.True(root.TryGetProperty("utc", out var utc));
        Assert.Equal(JsonValueKind.String, utc.ValueKind);
    }

    [Fact]
    public async Task Get_Config_Returns_Config_When_Enabled()
    {
        var response = await _client.GetAsync("/diagnostics/config");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("environmentName", out var environmentName));
        Assert.False(string.IsNullOrWhiteSpace(environmentName.GetString()));

        Assert.True(root.TryGetProperty("enableConfigEndpoint", out var enableConfigEndpoint));
        Assert.True(enableConfigEndpoint.GetBoolean());
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
