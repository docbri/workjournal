using WorkJournalApi.Domain;

namespace WorkJournalApi.Repositories;

public sealed class InMemoryWorkItemRepository : IWorkItemRepository
{
    private readonly List<WorkItem> _items = [];

    public Task<IReadOnlyList<WorkItem>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<WorkItem> items = _items
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();

        return Task.FromResult(items);
    }

    public Task<WorkItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = _items.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(item);
    }

    public Task AddAsync(WorkItem item, CancellationToken cancellationToken)
    {
        _items.Add(item);
        return Task.CompletedTask;
    }

    public Task SaveAsync(WorkItem item, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = _items.FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            return Task.FromResult(false);
        }

        var removed = _items.Remove(item);
        return Task.FromResult(removed);
    }
}
