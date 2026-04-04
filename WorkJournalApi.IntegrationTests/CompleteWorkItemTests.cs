using System.Net;
using System.Net.Http.Json;
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
        // Arrange: create a valid item first
        var createRequest = new CreateWorkItemRequest
        {
            Title = "Finish integration testing lesson",
            Notes = "This item will be completed."
        };

        var createResponse = await _client.PostAsJsonAsync("/work-items", createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.False(created.IsCompleted);
        Assert.Null(created.CompletedAtUtc);

        var completeRoute = $"/work-items/{created.Id}/complete";

        // Act: complete the item
        var completeResponse = await _client.PostAsync(completeRoute, content: null);

        // ✅ FIX: expect 204 NoContent
        Assert.Equal(HttpStatusCode.NoContent, completeResponse.StatusCode);

        // Act: fetch the item again to verify persistence
        var getResponse = await _client.GetAsync($"/work-items/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(createRequest.Title, fetched.Title);
        Assert.Equal(createRequest.Notes, fetched.Notes);

        // ✅ Critical assertions
        Assert.True(fetched.IsCompleted);
        Assert.NotNull(fetched.CompletedAtUtc);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private sealed class CreateWorkItemRequest
    {
        public string Title { get; init; } = string.Empty;
        public string? Notes { get; init; }
    }

    private sealed class WorkItemResponse
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Notes { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public bool IsCompleted { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
    }
}
