using MediatR;
using Microsoft.EntityFrameworkCore;
using SwiftERP.Finance.Domain.LedgerEntries;
using SwiftERP.Finance.Infrastructure.Persistence.Configurations;
using SwiftERP.Inventory.Domain.Products;
using SwiftERP.Inventory.Infrastructure.Persistence.Configurations;
using SwiftERP.Sales.Domain.SaleOrders;
using SwiftERP.Sales.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.Sales.Infrastructure.Persistence;

/// <summary>
/// Spans three modules' entities so that ConfirmSaleOrder can commit a stock decrement, a ledger
/// entry, and the order confirmation as a single database transaction (EF Core wraps one
/// SaveChangesAsync call in one transaction automatically). Product and LedgerEntry map to tables
/// owned by Inventory and Finance respectively — this context reuses their existing configurations
/// but is excluded from managing their schema (see ExcludeFromMigrations below); only SaleOrders and
/// SaleOrderLines are migrated from here.
/// </summary>
public class SalesDbContext(DbContextOptions<SalesDbContext> options, IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    public DbSet<SaleOrder> SaleOrders => Set<SaleOrder>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sales");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);

        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new LedgerEntryConfiguration());

        modelBuilder.Entity<Product>().ToTable(tb => tb.ExcludeFromMigrations());
        modelBuilder.Entity<LedgerEntry>().ToTable(tb => tb.ExcludeFromMigrations());
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        // Product is also tracked here (for the stock decrement in ConfirmSaleOrder), so a
        // ProductLowStockEvent raised during that decrement needs the same dispatch as
        // InventoryDbContext's own — otherwise a low-stock crossing caused by a sale would
        // silently never notify anyone.
        var trackedEntities = ChangeTracker.Entries<Entity>().Select(e => e.Entity).ToList();

        await base.SaveChangesAsync(cancellationToken);

        var events = DomainEventCollector.CollectAndClear(trackedEntities);
        foreach (var domainEvent in events)
            await publisher.Publish(domainEvent, cancellationToken);
    }
}
