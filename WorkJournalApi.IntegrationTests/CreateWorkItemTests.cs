using System.Net;
using System.Net.Http.Json;
using WorkJournalApi.IntegrationTests.Contracts;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class CreateWorkItemTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateWorkItemTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Post_WorkItem_Creates_Item_And_Returns_Created_Response()
    {
        // Arrange
        var request = new CreateWorkItemRequest
        {
            Title = "Write first integration test",
            Notes = "Verify POST create and follow-up GET by id."
        };

        // Act
        var postResponse = await _client.PostAsJsonAsync("/work-items", request);

        // Assert POST response status
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        // Assert Location header exists
        Assert.NotNull(postResponse.Headers.Location);

        // Assert response body
        var created = await postResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.Equal(request.Title, created.Title);
        Assert.Equal(request.Notes, created.Notes);
        Assert.False(created.IsCompleted);
        Assert.Null(created.CompletedAtUtc);

        // Act: follow the Location header to verify persistence
        var getResponse = await _client.GetAsync(postResponse.Headers.Location);

        // Assert GET response status
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(request.Title, fetched.Title);
        Assert.Equal(request.Notes, fetched.Notes);
        Assert.False(fetched.IsCompleted);
        Assert.Null(fetched.CompletedAtUtc);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
