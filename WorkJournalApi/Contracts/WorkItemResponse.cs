using WorkJournalApi.Domain;

namespace WorkJournalApi.Contracts;

public sealed class WorkItemResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public int Priority { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAtUtc { get; init; }

    public static WorkItemResponse FromDomain(WorkItem item) =>
        new()
        {
            Id = item.Id,
            Title = item.Title,
            Notes = item.Notes,
            Priority = item.Priority,
            CreatedAtUtc = item.CreatedAtUtc,
            IsCompleted = item.IsCompleted,
            CompletedAtUtc = item.CompletedAtUtc
        };
}
