using System.Net;
using System.Net.Http.Json;
using StackExchange.Redis;
using SwiftERP.Api.Endpoints;
using SwiftERP.Inventory.Domain.Products;
using SwiftERP.Inventory.Infrastructure.Persistence.Repositories;
using SwiftERP.Inventory.Integration.Tests.Infrastructure;

namespace SwiftERP.Inventory.Integration.Tests.Dashboard;

/// <summary>
/// Proves the cache-aside behavior described in Phase 4: the first request computes and caches
/// the summary in Redis; a subsequent write that crosses the reorder threshold explicitly
/// invalidates that cache entry (via ProductLowStockEvent -> DashboardCacheInvalidationHandler)
/// rather than waiting out the TTL.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class DashboardCacheTests(SqlServerContainerFixture fixture) : IAsyncLifetime
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
    public async Task GetDashboard_FirstCall_PopulatesRedisCache()
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(DashboardEndpoints.CacheKey);

        var response = await _client.GetAsync("/api/v1/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(await db.KeyExistsAsync(DashboardEndpoints.CacheKey));
    }

    [Fact]
    public async Task ProductCrossingLowStockThreshold_InvalidatesCachedDashboard()
    {
        var db = _redis.GetDatabase();

        // Warm the cache.
        await _client.GetAsync("/api/v1/dashboard");
        Assert.True(await db.KeyExistsAsync(DashboardEndpoints.CacheKey));

        var product = new Product($"SKU-{Guid.NewGuid():N}", "Cache Test Widget", reorderThreshold: 5, Guid.NewGuid(), initialQuantity: 6);
        await using (var dbContext = fixture.CreateInventoryDbContext())
        {
            new ProductRepository(dbContext).Add(product);
            await dbContext.SaveChangesAsync();
        }

        var adjustResponse = await _client.PutAsJsonAsync(
            $"/api/v1/inventory/products/{product.Id}/stock", new { NewQuantity = 2 });
        Assert.Equal(HttpStatusCode.NoContent, adjustResponse.StatusCode);

        // Give the fire-and-forget-from-the-request's-perspective-but-actually-awaited publish a
        // moment to land — Publish is awaited inside SaveChangesAsync itself, so this should
        // already be true immediately, but a short poll keeps the test robust either way.
        var invalidated = false;
        for (var i = 0; i < 20 && !invalidated; i++)
        {
            invalidated = !await db.KeyExistsAsync(DashboardEndpoints.CacheKey);
            if (!invalidated)
                await Task.Delay(50);
        }

        Assert.True(invalidated, "Expected the dashboard cache key to be removed after crossing the reorder threshold.");
    }
}
