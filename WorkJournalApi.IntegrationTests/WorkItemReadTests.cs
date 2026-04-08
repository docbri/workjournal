using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WorkJournalApi.IntegrationTests.Contracts;
using WorkJournalApi.IntegrationTests.Helpers;
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

    [Fact]
    public async Task Get_WorkItem_By_Id_Returns_The_Created_Item()
    {
        var created = await WorkItemTestHelper.CreateWorkItemAsync(
            _client,
            title: "Read by id title",
            notes: "Read by id notes");

        var route = $"/work-items/{created.Id}";

        var response = await _client.GetAsync(route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fetched = await response.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("Read by id title", fetched.Title);
        Assert.Equal("Read by id notes", fetched.Notes);
        Assert.False(fetched.IsCompleted);
        Assert.Null(fetched.CompletedAtUtc);
    }

    [Fact]
    public async Task Get_All_WorkItems_Returns_Items_In_Deterministic_Descending_Order()
    {
        await WorkItemTestHelper.CreateWorkItemAsync(
            _client,
            title: "First item",
            notes: "Created first");

        await WorkItemTestHelper.CreateWorkItemAsync(
            _client,
            title: "Second item",
            notes: "Created second");

        var response = await _client.GetAsync("/work-items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<WorkItemResponse>>();

        Assert.NotNull(items);
        Assert.True(items!.Count >= 2);

        for (var i = 0; i < items.Count - 1; i++)
        {
            var current = items[i];
            var next = items[i + 1];

            var isCorrectOrder =
                current.CreatedAtUtc > next.CreatedAtUtc ||
                (current.CreatedAtUtc == next.CreatedAtUtc && current.Id.CompareTo(next.Id) > 0);

            Assert.True(
                isCorrectOrder,
                $"Items are not ordered correctly at positions {i} and {i + 1}. " +
                $"Current: {current.Id} @ {current.CreatedAtUtc:o}, " +
                $"Next: {next.Id} @ {next.CreatedAtUtc:o}");
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
