using WorkJournalApi.Domain;

namespace WorkJournalApi.Services;

public interface IWorkItemService
{
    Task<IReadOnlyList<WorkItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<WorkItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<WorkItem> CreateAsync(string title, string? notes, CancellationToken cancellationToken);
    Task<WorkItem?> UpdateAsync(Guid id, string title, string? notes, CancellationToken cancellationToken);
    Task<bool> CompleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
