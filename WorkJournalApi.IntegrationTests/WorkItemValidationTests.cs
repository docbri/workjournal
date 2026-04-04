using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WorkJournalApi.IntegrationTests.Contracts;
using WorkJournalApi.IntegrationTests.Helpers;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class WorkItemValidationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public WorkItemValidationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Post_WorkItem_With_Empty_Title_Returns_BadRequest()
    {
        var request = new CreateWorkItemRequest
        {
            Title = "",
            Notes = "This should fail validation."
        };

        var response = await _client.PostAsJsonAsync("/work-items", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var content = await response.Content.ReadAsStringAsync();

        Assert.False(string.IsNullOrWhiteSpace(content));

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("title", out var titleProperty));
        Assert.False(string.IsNullOrWhiteSpace(titleProperty.GetString()));

        Assert.True(root.TryGetProperty("status", out var statusProperty));
        Assert.Equal(400, statusProperty.GetInt32());

        Assert.True(root.TryGetProperty("errors", out var errorsProperty));
        Assert.Equal(JsonValueKind.Object, errorsProperty.ValueKind);

        var errorEntries = errorsProperty.EnumerateObject().ToList();
        Assert.NotEmpty(errorEntries);

        var firstErrorEntry = errorEntries[0];
        Assert.False(string.IsNullOrWhiteSpace(firstErrorEntry.Name));
        Assert.Equal(JsonValueKind.Array, firstErrorEntry.Value.ValueKind);

        var messages = firstErrorEntry.Value.EnumerateArray().ToList();
        Assert.NotEmpty(messages);
        Assert.All(messages, message => Assert.False(string.IsNullOrWhiteSpace(message.GetString())));
    }

    [Fact]
    public async Task Put_WorkItem_With_Empty_Title_Returns_BadRequest_And_Does_Not_Change_Item()
    {
        var created = await WorkItemTestHelper.CreateWorkItemAsync(
            _client,
            title: "Original title",
            notes: "Original notes");

        var invalidUpdateRequest = new UpdateWorkItemRequest
        {
            Title = "",
            Notes = "This update should fail validation."
        };

        var route = $"/work-items/{created.Id}";

        var updateResponse = await _client.PutAsJsonAsync(route, invalidUpdateRequest);

        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
        Assert.Equal("application/problem+json", updateResponse.Content.Headers.ContentType?.MediaType);

        var problemContent = await updateResponse.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(problemContent));

        using (var problemDocument = JsonDocument.Parse(problemContent))
        {
            var root = problemDocument.RootElement;

            Assert.True(root.TryGetProperty("title", out var titleProperty));
            Assert.False(string.IsNullOrWhiteSpace(titleProperty.GetString()));

            Assert.True(root.TryGetProperty("status", out var statusProperty));
            Assert.Equal(400, statusProperty.GetInt32());

            Assert.True(root.TryGetProperty("errors", out var errorsProperty));
            Assert.Equal(JsonValueKind.Object, errorsProperty.ValueKind);

            var errorEntries = errorsProperty.EnumerateObject().ToList();
            Assert.NotEmpty(errorEntries);

            var firstErrorEntry = errorEntries[0];
            Assert.False(string.IsNullOrWhiteSpace(firstErrorEntry.Name));
            Assert.Equal(JsonValueKind.Array, firstErrorEntry.Value.ValueKind);

            var messages = firstErrorEntry.Value.EnumerateArray().ToList();
            Assert.NotEmpty(messages);
            Assert.All(messages, message => Assert.False(string.IsNullOrWhiteSpace(message.GetString())));
        }

        var getResponse = await _client.GetAsync(route);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("Original title", fetched.Title);
        Assert.Equal("Original notes", fetched.Notes);
        Assert.False(fetched.IsCompleted);
        Assert.Null(fetched.CompletedAtUtc);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
