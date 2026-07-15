namespace SwiftERP.Api.Notifications;

/// <summary>
/// Publishes cross-cutting real-time notifications (low stock, payroll processed, ...) to
/// whatever transport the API is configured with. Lives at the API layer, not in any module's
/// Application layer, since "broadcast to Redis pub/sub" is a presentation/infrastructure concern
/// no module should need to know about.
/// </summary>
public interface INotificationPublisher
{
    Task PublishAsync(string eventType, object payload, CancellationToken cancellationToken);
}
