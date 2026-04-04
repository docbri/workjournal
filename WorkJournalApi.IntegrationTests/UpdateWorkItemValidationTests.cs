using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WorkJournalApi.IntegrationTests.Contracts;
using WorkJournalApi.IntegrationTests.Helpers;
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
        // Arrange
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

        // Act
        var updateResponse = await _client.PutAsJsonAsync(route, invalidUpdateRequest);

        // Assert rejected response
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

        // Act: fetch item again to verify it was not changed
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
