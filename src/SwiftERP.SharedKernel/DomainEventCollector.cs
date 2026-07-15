namespace SwiftERP.SharedKernel;

/// <summary>
/// Pure helper (no EF Core dependency, so it's safe to live in the shared kernel) for the
/// "collect domain events raised during this unit of work, then clear them" step every module's
/// DbContext performs after a successful SaveChangesAsync, before publishing them via MediatR.
/// </summary>
public static class DomainEventCollector
{
    public static IReadOnlyList<IDomainEvent> CollectAndClear(IEnumerable<Entity> entities)
    {
        var entityList = entities.ToList();
        var events = entityList.SelectMany(e => e.DomainEvents).ToList();

        foreach (var entity in entityList)
            entity.ClearDomainEvents();

        return events;
    }
}
