using System.Net;
using System.Net.Http.Json;
using WorkJournalApi.IntegrationTests.Contracts;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class WorkItemNotFoundTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public WorkItemNotFoundTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_WorkItem_With_Unknown_Id_Returns_NotFound()
    {
        var unknownId = Guid.NewGuid();
        var route = $"/work-items/{unknownId}";

        var response = await _client.GetAsync(route);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_WorkItem_With_Unknown_Id_Returns_NotFound()
    {
        var unknownId = Guid.NewGuid();
        var route = $"/work-items/{unknownId}";

        var request = new UpdateWorkItemRequest
        {
            Title = "Updated title",
            Notes = "Updated notes"
        };

        var response = await _client.PutAsJsonAsync(route, request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_Complete_WorkItem_With_Unknown_Id_Returns_NotFound()
    {
        var unknownId = Guid.NewGuid();
        var route = $"/work-items/{unknownId}/complete";

        var response = await _client.PostAsync(route, content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WorkItem_With_Unknown_Id_Returns_NotFound()
    {
        var unknownId = Guid.NewGuid();
        var route = $"/work-items/{unknownId}";

        var response = await _client.DeleteAsync(route);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
