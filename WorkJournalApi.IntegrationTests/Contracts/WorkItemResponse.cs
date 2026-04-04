namespace WorkJournalApi.IntegrationTests.Contracts;

public sealed class WorkItemResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
}
