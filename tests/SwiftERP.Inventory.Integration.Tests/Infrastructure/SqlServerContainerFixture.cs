using Microsoft.EntityFrameworkCore;
using SwiftERP.Finance.Infrastructure.Persistence;
using SwiftERP.HR.Infrastructure.Persistence;
using SwiftERP.Inventory.Infrastructure.Persistence;
using SwiftERP.Sales.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace SwiftERP.Inventory.Integration.Tests.Infrastructure;

// Name kept as SqlServerContainerFixture/SqlServerCollection despite now running Postgres — this
// is test-project-internal naming, not worth a mass rename across every test file mid-migration.
public class SqlServerContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _sqlContainer =
        new PostgreSqlBuilder("postgres:17-alpine").Build();

    private readonly RedisContainer _redisContainer =
        new RedisBuilder("redis:7-alpine").Build();

    public string ConnectionString { get; private set; } = default!;
    public string RedisConnectionString { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_sqlContainer.StartAsync(), _redisContainer.StartAsync());

        ConnectionString = _sqlContainer.GetConnectionString();
        RedisConnectionString = _redisContainer.GetConnectionString();

        // Order matters: Inventory owns Products, Finance owns LedgerEntries. SalesDbContext maps
        // both as ExcludeFromMigrations, so its own migration only creates SaleOrders/SaleOrderLines
        // once those owning tables already exist.
        await using (var inventoryDbContext = CreateInventoryDbContext())
            await inventoryDbContext.Database.MigrateAsync();

        await using (var financeDbContext = CreateFinanceDbContext())
            await financeDbContext.Database.MigrateAsync();

        await using (var salesDbContext = CreateSalesDbContext())
            await salesDbContext.Database.MigrateAsync();

        await using (var hrDbContext = CreateHrDbContext())
            await hrDbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(_sqlContainer.DisposeAsync().AsTask(), _redisContainer.DisposeAsync().AsTask());
    }

    private static readonly NoOpPublisher Publisher = new();

    public InventoryDbContext CreateDbContext() => CreateInventoryDbContext();

    public InventoryDbContext CreateInventoryDbContext() =>
        new(new DbContextOptionsBuilder<InventoryDbContext>().UseNpgsql(ConnectionString).Options, Publisher);

    public FinanceDbContext CreateFinanceDbContext() =>
        new(new DbContextOptionsBuilder<FinanceDbContext>().UseNpgsql(ConnectionString).Options);

    public SalesDbContext CreateSalesDbContext() =>
        new(new DbContextOptionsBuilder<SalesDbContext>().UseNpgsql(ConnectionString).Options, Publisher);

    public HrDbContext CreateHrDbContext() =>
        new(new DbContextOptionsBuilder<HrDbContext>().UseNpgsql(ConnectionString).Options, Publisher);
}

[CollectionDefinition(Name)]
public class SqlServerCollection : ICollectionFixture<SqlServerContainerFixture>
{
    public const string Name = "SqlServer collection";
}
