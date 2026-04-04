using WorkJournalApi.Domain;
using WorkJournalApi.Repositories;

namespace WorkJournalApi.Services;

public sealed class WorkItemService : IWorkItemService
{
    private readonly IWorkItemRepository _repository;
    private readonly ILogger<WorkItemService> _logger;

    public WorkItemService(
        IWorkItemRepository repository,
        ILogger<WorkItemService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<IReadOnlyList<WorkItem>> GetAllAsync(CancellationToken cancellationToken) =>
        _repository.GetAllAsync(cancellationToken);

    public Task<WorkItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _repository.GetByIdAsync(id, cancellationToken);

    public async Task<WorkItem> CreateAsync(string title, string? notes, CancellationToken cancellationToken)
    {
        var item = new WorkItem(title, notes);

        await _repository.AddAsync(item, cancellationToken);

        _logger.LogInformation(
            "Created work item {WorkItemId} with title {Title}",
            item.Id,
            item.Title);

        return item;
    }

    public async Task<WorkItem?> UpdateAsync(Guid id, string title, string? notes, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return null;
        }

        item.UpdateDetails(title, notes);
        await _repository.SaveAsync(item, cancellationToken);

        _logger.LogInformation("Updated work item {WorkItemId}", id);

        return item;
    }

    public async Task<bool> CompleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return false;
        }

        item.MarkComplete();
        await _repository.SaveAsync(item, cancellationToken);

        _logger.LogInformation("Completed work item {WorkItemId}", id);

        return true;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        _repository.DeleteAsync(id, cancellationToken);
}
