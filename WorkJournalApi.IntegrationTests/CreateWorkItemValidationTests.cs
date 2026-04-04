using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WorkJournalApi.IntegrationTests.Contracts;
using Xunit;

namespace WorkJournalApi.IntegrationTests;

public sealed class CreateWorkItemValidationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateWorkItemValidationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Post_WorkItem_With_Empty_Title_Returns_BadRequest()
    {
        // Arrange
        var request = new CreateWorkItemRequest
        {
            Title = "",
            Notes = "This should fail validation."
        };

        // Act
        var response = await _client.PostAsJsonAsync("/work-items", request);

        // Assert status code
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Assert content type (Problem Details)
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var content = await response.Content.ReadAsStringAsync();

        Assert.False(string.IsNullOrWhiteSpace(content));

        using var document = JsonDocument.Parse(content);

        var root = document.RootElement;

        // Assert basic Problem Details structure
        Assert.True(root.TryGetProperty("title", out _));
        Assert.True(root.TryGetProperty("status", out _));
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
