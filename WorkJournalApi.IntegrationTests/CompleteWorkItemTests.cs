using System.Net;
using System.Net.Http.Json;
using WorkJournalApi.IntegrationTests.Contracts;
using WorkJournalApi.IntegrationTests.Helpers;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class CompleteWorkItemTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CompleteWorkItemTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Post_Complete_WorkItem_Marks_Item_As_Completed_And_Persists_Changes()
    {
        // Arrange
        var created = await WorkItemTestHelper.CreateWorkItemAsync(
            _client,
            title: "Finish integration testing lesson",
            notes: "This item will be completed.");

        Assert.False(created.IsCompleted);
        Assert.Null(created.CompletedAtUtc);

        var completeRoute = $"/work-items/{created.Id}/complete";

        // Act
        var completeResponse = await _client.PostAsync(completeRoute, content: null);

        // Assert command response
        Assert.Equal(HttpStatusCode.NoContent, completeResponse.StatusCode);

        // Act: fetch item again to verify persistence
        var getResponse = await _client.GetAsync($"/work-items/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("Finish integration testing lesson", fetched.Title);
        Assert.Equal("This item will be completed.", fetched.Notes);
        Assert.True(fetched.IsCompleted);
        Assert.NotNull(fetched.CompletedAtUtc);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
