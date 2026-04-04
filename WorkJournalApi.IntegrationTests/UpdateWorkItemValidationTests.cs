using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class UpdateWorkItemValidationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UpdateWorkItemValidationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Put_WorkItem_With_Empty_Title_Returns_BadRequest_And_Does_Not_Change_Item()
    {
        // Arrange: create a valid item first
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

        var invalidUpdateRequest = new UpdateWorkItemRequest
        {
            Title = "",
            Notes = "This update should fail validation."
        };

        var route = $"/work-items/{created.Id}";

        // Act: attempt invalid update
        var updateResponse = await _client.PutAsJsonAsync(route, invalidUpdateRequest);

        // Assert: request is rejected
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
        Assert.Equal("application/problem+json", updateResponse.Content.Headers.ContentType?.MediaType);

        var problemContent = await updateResponse.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(problemContent));

        using (var problemDocument = JsonDocument.Parse(problemContent))
        {
            var root = problemDocument.RootElement;

            Assert.True(root.TryGetProperty("title", out _));
            Assert.True(root.TryGetProperty("status", out _));
        }

        // Act: fetch the item again to verify it was not changed
        var getResponse = await _client.GetAsync(route);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(createRequest.Title, fetched.Title);
        Assert.Equal(createRequest.Notes, fetched.Notes);
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
