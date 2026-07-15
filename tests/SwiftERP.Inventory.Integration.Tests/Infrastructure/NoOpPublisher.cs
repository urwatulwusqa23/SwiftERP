using MediatR;

namespace SwiftERP.Inventory.Integration.Tests.Infrastructure;

/// <summary>
/// DbContexts constructed directly by the test fixture (bypassing the DI container) need an
/// IPublisher to satisfy the constructor — domain events raised via this path aren't under test,
/// so publishing is a no-op here rather than wiring up a real MediatR pipeline for it.
/// </summary>
public class NoOpPublisher : IPublisher
{
    public Task Publish(object notification, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification =>
        Task.CompletedTask;
}
