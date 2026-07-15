using System.Net.Http.Json;
using StackExchange.Redis;
using SwiftERP.HR.Application.Employees.HireEmployee;
using SwiftERP.HR.Application.Payroll.CreatePayrollRun;
using SwiftERP.Inventory.Domain.Products;
using SwiftERP.Inventory.Infrastructure.Persistence.Repositories;
using SwiftERP.Inventory.Integration.Tests.Infrastructure;

namespace SwiftERP.Inventory.Integration.Tests.Dashboard;

/// <summary>
/// Proves the Redis pub/sub side of Phase 4: a real subscriber on the notifications channel
/// actually receives a message when a ProductLowStockEvent or PayrollRunPostedEvent is raised
/// and dispatched through RedisNotificationPublisher — not just that the handler was invoked.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class NotificationPublishingTests(SqlServerContainerFixture fixture) : IAsyncLifetime
{
    private SwiftErpApiFactory _factory = default!;
    private HttpClient _client = default!;
    private ConnectionMultiplexer _redis = default!;

    public async Task InitializeAsync()
    {
        _factory = new SwiftErpApiFactory(fixture.ConnectionString, fixture.RedisConnectionString);
        _client = _factory.CreateClient();
        _redis = await ConnectionMultiplexer.ConnectAsync(fixture.RedisConnectionString);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _redis.DisposeAsync();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task DecrementingStockBelowThreshold_PublishesStockLowNotification()
    {
        var receivedMessages = new List<string>();
        var subscriber = _redis.GetSubscriber();
        await subscriber.SubscribeAsync(RedisChannel.Literal("swifterp:notifications"), (_, message) =>
        {
            lock (receivedMessages)
                receivedMessages.Add(message!);
        });

        var product = new Product($"SKU-{Guid.NewGuid():N}", "Notify Test Widget", reorderThreshold: 5, Guid.NewGuid(), initialQuantity: 6);
        await using (var dbContext = fixture.CreateInventoryDbContext())
        {
            new ProductRepository(dbContext).Add(product);
            await dbContext.SaveChangesAsync();
        }

        await _client.PutAsJsonAsync($"/api/v1/inventory/products/{product.Id}/stock", new { NewQuantity = 2 });

        var found = await WaitForMessageContainingAsync(receivedMessages, "stock.low");

        Assert.True(found, "Expected a 'stock.low' notification on the Redis pub/sub channel.");
    }

    [Fact]
    public async Task PostingPayrollRun_PublishesPayrollProcessedNotification()
    {
        var receivedMessages = new List<string>();
        var subscriber = _redis.GetSubscriber();
        await subscriber.SubscribeAsync(RedisChannel.Literal("swifterp:notifications"), (_, message) =>
        {
            lock (receivedMessages)
                receivedMessages.Add(message!);
        });

        var hireResponse = await _client.PostAsJsonAsync(
            "/api/v1/hr/employees",
            new HireEmployeeCommand($"Notify {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com", 4000m, new DateOnly(2026, 1, 1)));
        hireResponse.EnsureSuccessStatusCode();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/hr/payroll-runs", new CreatePayrollRunCommand(2026, 9));
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        await _client.PostAsync($"/api/v1/hr/payroll-runs/{created!.Id}/post", null);

        var found = await WaitForMessageContainingAsync(receivedMessages, "payroll.processed");

        Assert.True(found, "Expected a 'payroll.processed' notification on the Redis pub/sub channel.");
    }

    private static async Task<bool> WaitForMessageContainingAsync(List<string> messages, string substring)
    {
        for (var i = 0; i < 40; i++)
        {
            lock (messages)
            {
                if (messages.Any(m => m.Contains(substring)))
                    return true;
            }

            await Task.Delay(50);
        }

        return false;
    }

    private record CreatedResponse(Guid Id);
}
