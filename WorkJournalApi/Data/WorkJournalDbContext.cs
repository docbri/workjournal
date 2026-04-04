using Microsoft.EntityFrameworkCore;
using WorkJournalApi.Domain;

namespace WorkJournalApi.Data;

public sealed class WorkJournalDbContext : DbContext
{
    public WorkJournalDbContext(DbContextOptions<WorkJournalDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var workItem = modelBuilder.Entity<WorkItem>();

        workItem.HasKey(x => x.Id);

        workItem.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(120);

        workItem.Property(x => x.Notes)
            .HasMaxLength(1000);

        workItem.Property(x => x.CreatedAtUtc)
            .IsRequired();

        workItem.Property(x => x.IsCompleted)
            .IsRequired();

        workItem.Property(x => x.CompletedAtUtc);
    }
}
