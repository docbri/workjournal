namespace WorkJournalApi.Contracts;

public sealed class UpdateWorkItemRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
