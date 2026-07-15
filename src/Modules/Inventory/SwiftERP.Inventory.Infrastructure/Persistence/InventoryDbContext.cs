using MediatR;
using Microsoft.EntityFrameworkCore;
using SwiftERP.Inventory.Domain.Products;
using SwiftERP.Inventory.Domain.PurchaseOrders;
using SwiftERP.Inventory.Domain.Shared;
using SwiftERP.Inventory.Domain.Suppliers;
using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Infrastructure.Persistence;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options, IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventory");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        var trackedEntities = ChangeTracker.Entries<Entity>().Select(e => e.Entity).ToList();

        await base.SaveChangesAsync(cancellationToken);

        // Published only after the transaction commits, so a subscriber never sees an event for
        // a change that ultimately rolled back.
        var events = DomainEventCollector.CollectAndClear(trackedEntities);
        foreach (var domainEvent in events)
            await publisher.Publish(domainEvent, cancellationToken);
    }
}
