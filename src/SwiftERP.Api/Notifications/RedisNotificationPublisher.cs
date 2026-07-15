using System.Text.Json;
using StackExchange.Redis;

namespace SwiftERP.Api.Notifications;

public class RedisNotificationPublisher(IConnectionMultiplexer redis) : INotificationPublisher
{
    public const string ChannelName = "swifterp:notifications";

    public async Task PublishAsync(string eventType, object payload, CancellationToken cancellationToken)
    {
        var envelope = new { type = eventType, occurredAtUtc = DateTimeOffset.UtcNow, payload };
        var json = JsonSerializer.Serialize(envelope);

        var subscriber = redis.GetSubscriber();
        await subscriber.PublishAsync(RedisChannel.Literal(ChannelName), json);
    }
}
