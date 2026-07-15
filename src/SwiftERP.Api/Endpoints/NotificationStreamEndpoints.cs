using System.Text;
using StackExchange.Redis;
using SwiftERP.Api.Notifications;

namespace SwiftERP.Api.Endpoints;

public static class NotificationStreamEndpoints
{
    public static void MapNotificationStreamEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/notifications/stream", async (HttpContext context, IConnectionMultiplexer redis) =>
        {
            context.Response.Headers.ContentType = "text/event-stream";
            context.Response.Headers.CacheControl = "no-cache";
            context.Response.Headers["X-Accel-Buffering"] = "no";

            var subscriber = redis.GetSubscriber();
            var channel = await subscriber.SubscribeAsync(RedisChannel.Literal(RedisNotificationPublisher.ChannelName));

            var cancellationToken = context.RequestAborted;

            // Send a comment immediately so the browser's EventSource fires 'open' right away
            // instead of appearing to hang until the first real notification.
            await context.Response.WriteAsync(": connected\n\n", cancellationToken);
            await context.Response.Body.FlushAsync(cancellationToken);

            try
            {
                await foreach (var message in channel.WithCancellation(cancellationToken))
                {
                    var payload = Encoding.UTF8.GetString(message.Message!);
                    await context.Response.WriteAsync($"data: {payload}\n\n", cancellationToken);
                    await context.Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Client disconnected — normal shutdown of the stream.
            }
            finally
            {
                await channel.UnsubscribeAsync();
            }
        }).WithTags("Notifications");
    }
}
