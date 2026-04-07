namespace WorkJournalApi.Domain;

public sealed class WorkItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public int Priority { get; private set; }

    private WorkItem()
    {
    }

    public WorkItem(string title, string? notes, int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Id = Guid.NewGuid();
        Title = title.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        CreatedAtUtc = DateTime.UtcNow;
        Priority = priority;
    }

    public void MarkComplete()
    {
        if (IsCompleted) return;

        IsCompleted = true;
        CompletedAtUtc = DateTime.UtcNow;
    }
    
    public void UpdateDetails(string title, string? notes)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Title = title.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
