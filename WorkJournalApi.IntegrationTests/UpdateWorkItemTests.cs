using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class UpdateWorkItemTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UpdateWorkItemTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Put_WorkItem_Updates_Item_And_Persists_Changes()
    {
        // Arrange: create an item first
        var createRequest = new CreateWorkItemRequest
        {
            Title = "Original title",
            Notes = "Original notes"
        };

        var createResponse = await _client.PostAsJsonAsync("/work-items", createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);

        var updateRequest = new UpdateWorkItemRequest
        {
            Title = "Updated title",
            Notes = "Updated notes"
        };

        // IMPORTANT:
        // If your actual update route is different, replace this route string.
        var updateRoute = $"/work-items/{created.Id}";

        // Act: update the item
        var updateResponse = await _client.PutAsJsonAsync(updateRoute, updateRequest);

        // Assert update response status
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated!.Id);
        Assert.Equal(updateRequest.Title, updated.Title);
        Assert.Equal(updateRequest.Notes, updated.Notes);
        Assert.False(updated.IsCompleted);
        Assert.Null(updated.CompletedAtUtc);

        // Act: GET the item again to verify persistence
        var getResponse = await _client.GetAsync(updateRoute);

        // Assert GET response status
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(updateRequest.Title, fetched.Title);
        Assert.Equal(updateRequest.Notes, fetched.Notes);
        Assert.False(fetched.IsCompleted);
        Assert.Null(fetched.CompletedAtUtc);
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

    private sealed class UpdateWorkItemRequest
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
