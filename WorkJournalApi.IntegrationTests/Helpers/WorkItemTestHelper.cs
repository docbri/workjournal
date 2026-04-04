using System.Net;
using System.Net.Http.Json;
using WorkJournalApi.IntegrationTests.Contracts;
using Xunit;

namespace WorkJournalApi.IntegrationTests.Helpers;

public static class WorkItemTestHelper
{
    public static async Task<WorkItemResponse> CreateWorkItemAsync(
        HttpClient client,
        string title = "Test work item",
        string? notes = "Created by test helper.")
    {
        var request = new CreateWorkItemRequest
        {
            Title = title,
            Notes = notes
        };

        var response = await client.PostAsJsonAsync("/work-items", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<WorkItemResponse>();

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);

        return created;
    }
}
