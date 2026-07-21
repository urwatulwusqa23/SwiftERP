namespace SwiftERP.Api.Notifications;

/// <summary>
/// Used when no Redis connection string is configured — live notifications just don't go
/// anywhere. The dashboard's numbers still update on refresh either way; only the "instant" push
/// via SSE is lost, which is an acceptable degradation rather than a hard dependency.
/// </summary>
public class NoOpNotificationPublisher : INotificationPublisher
{
    public Task PublishAsync(string eventType, object payload, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
