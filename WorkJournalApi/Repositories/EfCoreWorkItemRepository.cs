using Microsoft.EntityFrameworkCore;
using WorkJournalApi.Data;
using WorkJournalApi.Domain;

namespace WorkJournalApi.Repositories;

public sealed class EfCoreWorkItemRepository : IWorkItemRepository
{
    private readonly WorkJournalDbContext _dbContext;

    public EfCoreWorkItemRepository(WorkJournalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<WorkItem>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.WorkItems
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.WorkItems
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(WorkItem item, CancellationToken cancellationToken)
    {
        await _dbContext.WorkItems.AddAsync(item, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(WorkItem item, CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _dbContext.WorkItems
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (item is null)
        {
            return false;
        }

        _dbContext.WorkItems.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
