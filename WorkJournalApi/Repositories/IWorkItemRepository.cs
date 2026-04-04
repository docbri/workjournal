using WorkJournalApi.Domain;

namespace WorkJournalApi.Repositories;

public interface IWorkItemRepository
{
    Task<IReadOnlyList<WorkItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<WorkItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(WorkItem item, CancellationToken cancellationToken);
    Task SaveAsync(WorkItem item, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
