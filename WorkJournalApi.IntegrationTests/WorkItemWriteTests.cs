using System.Net;
using System.Net.Http.Json;
using WorkJournalApi.IntegrationTests.Contracts;
using WorkJournalApi.IntegrationTests.Helpers;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class WorkItemWriteTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public WorkItemWriteTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Post_WorkItem_Creates_Item_And_Returns_Created_Response()
    {
        var request = new CreateWorkItemRequest
        {
            Title = "Write first integration test",
            Notes = "Verify POST create and follow-up GET by id."
        };

        var postResponse = await _client.PostAsJsonAsync("/work-items", request);

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);

        var created = await postResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.Equal(request.Title, created.Title);
        Assert.Equal(request.Notes, created.Notes);
        Assert.False(created.IsCompleted);
        Assert.Null(created.CompletedAtUtc);

        var getResponse = await _client.GetAsync(postResponse.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(request.Title, fetched.Title);
        Assert.Equal(request.Notes, fetched.Notes);
        Assert.False(fetched.IsCompleted);
        Assert.Null(fetched.CompletedAtUtc);
    }

    [Fact]
    public async Task Put_WorkItem_Updates_Item_And_Persists_Changes()
    {
        var created = await WorkItemTestHelper.CreateWorkItemAsync(
            _client,
            title: "Original title",
            notes: "Original notes");

        var updateRequest = new UpdateWorkItemRequest
        {
            Title = "Updated title",
            Notes = "Updated notes"
        };

        var updateRoute = $"/work-items/{created.Id}";

        var updateResponse = await _client.PutAsJsonAsync(updateRoute, updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated!.Id);
        Assert.Equal(updateRequest.Title, updated.Title);
        Assert.Equal(updateRequest.Notes, updated.Notes);
        Assert.False(updated.IsCompleted);
        Assert.Null(updated.CompletedAtUtc);

        var getResponse = await _client.GetAsync(updateRoute);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(updateRequest.Title, fetched.Title);
        Assert.Equal(updateRequest.Notes, fetched.Notes);
        Assert.False(fetched.IsCompleted);
        Assert.Null(fetched.CompletedAtUtc);
    }

    [Fact]
    public async Task Post_Complete_WorkItem_Marks_Item_As_Completed_And_Persists_Changes()
    {
        var created = await WorkItemTestHelper.CreateWorkItemAsync(
            _client,
            title: "Finish integration testing lesson",
            notes: "This item will be completed.");

        Assert.False(created.IsCompleted);
        Assert.Null(created.CompletedAtUtc);

        var completeRoute = $"/work-items/{created.Id}/complete";

        var completeResponse = await _client.PostAsync(completeRoute, content: null);

        Assert.Equal(HttpStatusCode.NoContent, completeResponse.StatusCode);

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

    [Fact]
    public async Task Delete_WorkItem_Removes_Item_And_Item_Can_No_Longer_Be_Retrieved()
    {
        var created = await WorkItemTestHelper.CreateWorkItemAsync(
            _client,
            title: "Work item to delete",
            notes: "This item should be deleted.");

        var route = $"/work-items/{created.Id}";

        var deleteResponse = await _client.DeleteAsync(route);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync(route);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
