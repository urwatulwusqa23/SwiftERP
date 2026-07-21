using System.Text;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using SwiftERP.Api.Notifications;

namespace SwiftERP.Api.Endpoints;

public static class NotificationStreamEndpoints
{
    public static void MapNotificationStreamEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/notifications/stream", async (HttpContext context) =>
        {
            // Resolved manually rather than via a minimal-API parameter, since a parameter typed
            // IConnectionMultiplexer? still gets bound with GetRequiredService when there's no
            // registration at all to prove the type is nullable-optional — GetService here is
            // unambiguous either way.
            var redis = context.RequestServices.GetService<IConnectionMultiplexer>();

            context.Response.Headers.ContentType = "text/event-stream";
            context.Response.Headers.CacheControl = "no-cache";
            context.Response.Headers["X-Accel-Buffering"] = "no";

            var cancellationToken = context.RequestAborted;

            // Send a comment immediately so the browser's EventSource fires 'open' right away
            // instead of appearing to hang until the first real notification.
            await context.Response.WriteAsync(": connected\n\n", cancellationToken);
            await context.Response.Body.FlushAsync(cancellationToken);

            // No Redis configured — hold the connection open (so the client's EventSource doesn't
            // spin in a reconnect loop) but never emit anything. The dashboard's numbers still
            // update on refresh; only the instant push is unavailable.
            if (redis is null)
            {
                try
                {
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Client disconnected — normal shutdown of the stream.
                }
                return;
            }

            var subscriber = redis.GetSubscriber();
            var channel = await subscriber.SubscribeAsync(RedisChannel.Literal(RedisNotificationPublisher.ChannelName));

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
        }).WithTags("Notifications").RequireAuthorization();
    }
}
