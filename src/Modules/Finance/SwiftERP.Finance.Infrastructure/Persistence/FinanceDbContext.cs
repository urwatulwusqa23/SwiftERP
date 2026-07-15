using Microsoft.EntityFrameworkCore;
using SwiftERP.Finance.Domain.LedgerEntries;
using SwiftERP.Finance.Domain.Shared;

namespace SwiftERP.Finance.Infrastructure.Persistence;

public class FinanceDbContext(DbContextOptions<FinanceDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("finance");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceDbContext).Assembly);
    }

    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        base.SaveChangesAsync(cancellationToken);
}
