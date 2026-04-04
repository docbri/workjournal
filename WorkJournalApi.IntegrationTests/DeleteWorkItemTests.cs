using System.Net;
using WorkJournalApi.IntegrationTests.Helpers;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class DeleteWorkItemTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeleteWorkItemTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Delete_WorkItem_Removes_Item_And_Item_Can_No_Longer_Be_Retrieved()
    {
        // Arrange
        var created = await WorkItemTestHelper.CreateWorkItemAsync(
            _client,
            title: "Work item to delete",
            notes: "This item should be deleted.");

        var route = $"/work-items/{created.Id}";

        // Act: delete the item
        var deleteResponse = await _client.DeleteAsync(route);

        // Assert delete response
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Act: try to fetch the item afterward
        var getResponse = await _client.GetAsync(route);

        // Assert it is gone
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WorkItem_With_Unknown_Id_Returns_NotFound()
    {
        // Arrange
        var unknownId = Guid.NewGuid();
        var route = $"/work-items/{unknownId}";

        // Act
        var response = await _client.DeleteAsync(route);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
