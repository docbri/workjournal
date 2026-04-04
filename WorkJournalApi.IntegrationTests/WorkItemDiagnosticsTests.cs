using System.Net;
using System.Text.Json;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class WorkItemDiagnosticsTests
{
    [Fact]
    public async Task Get_Health_Returns_Healthy_Status()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

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
        using var factory = new CustomWebApplicationFactory(new Dictionary<string, string?>
        {
            ["Diagnostics:EnvironmentName"] = "IntegrationTesting",
            ["Diagnostics:EnableConfigEndpoint"] = "true"
        });

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/diagnostics/config");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("environmentName", out var environmentName));
        Assert.Equal("IntegrationTesting", environmentName.GetString());

        Assert.True(root.TryGetProperty("enableConfigEndpoint", out var enableConfigEndpoint));
        Assert.True(enableConfigEndpoint.GetBoolean());
    }

    [Fact]
    public async Task Get_Config_Returns_NotFound_When_Disabled()
    {
        using var factory = new CustomWebApplicationFactory(new Dictionary<string, string?>
        {
            ["Diagnostics:EnvironmentName"] = "IntegrationTesting",
            ["Diagnostics:EnableConfigEndpoint"] = "false"
        });

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/diagnostics/config");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
