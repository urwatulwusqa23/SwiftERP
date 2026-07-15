using MediatR;
using Microsoft.EntityFrameworkCore;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.Identity.Domain.Shared;
using SwiftERP.Identity.Domain.Users;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Infrastructure.Persistence;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options, IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        var trackedEntities = ChangeTracker.Entries<Entity>().Select(e => e.Entity).ToList();

        await base.SaveChangesAsync(cancellationToken);

        var events = DomainEventCollector.CollectAndClear(trackedEntities);
        foreach (var domainEvent in events)
            await publisher.Publish(domainEvent, cancellationToken);
    }
}
