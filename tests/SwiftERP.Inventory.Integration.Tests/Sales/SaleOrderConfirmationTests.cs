using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SwiftERP.Inventory.Domain.Products;
using SwiftERP.Inventory.Integration.Tests.Infrastructure;
using SwiftERP.Inventory.Infrastructure.Persistence.Repositories;
using SwiftERP.Sales.Application.SaleOrders.CreateSaleOrder;
using SwiftERP.Sales.Application.SaleOrders.GetSaleOrder;

namespace SwiftERP.Inventory.Integration.Tests.Sales;

/// <summary>
/// Proves the project's core cross-module claim: confirming a sale order decrements Inventory
/// stock and posts a Finance ledger entry as a single atomic transaction. Insufficient stock must
/// leave BOTH the Product and the ledger untouched — not just the Product.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class SaleOrderConfirmationTests(SqlServerContainerFixture fixture) : IAsyncLifetime
{
    private SwiftErpApiFactory _factory = default!;
    private HttpClient _client = default!;

    public Task InitializeAsync()
    {
        _factory = new SwiftErpApiFactory(fixture.ConnectionString, fixture.RedisConnectionString);
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<Product> SeedProductAsync(int initialQuantity)
    {
        var product = new Product($"SKU-{Guid.NewGuid():N}", "Cross-Module Widget", reorderThreshold: 0, Guid.NewGuid(), initialQuantity);

        await using var dbContext = fixture.CreateInventoryDbContext();
        new ProductRepository(dbContext).Add(product);
        await dbContext.SaveChangesAsync();

        return product;
    }

    private async Task<Guid> CreateSaleOrderAsync(Guid productId, int quantity, decimal unitPrice)
    {
        var command = new CreateSaleOrderCommand(
            Guid.NewGuid(), [new CreateSaleOrderLineDto(productId, quantity, unitPrice)]);

        var response = await _client.PostAsJsonAsync("/api/v1/sales/orders", command);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        return created!.Id;
    }

    [Fact]
    public async Task ConfirmSaleOrder_WithInsufficientStock_RollsBackBothStockAndLedger()
    {
        var product = await SeedProductAsync(initialQuantity: 5);
        var saleOrderId = await CreateSaleOrderAsync(product.Id, quantity: 10, unitPrice: 20m);

        var confirmResponse = await _client.PostAsync($"/api/v1/sales/orders/{saleOrderId}/confirm", null);

        Assert.Equal(HttpStatusCode.BadRequest, confirmResponse.StatusCode);

        await using var inventoryContext = fixture.CreateInventoryDbContext();
        var persistedProduct = await new ProductRepository(inventoryContext)
            .GetByIdAsync(product.Id, CancellationToken.None);
        Assert.Equal(5, persistedProduct!.QuantityOnHand);

        await using var financeContext = fixture.CreateFinanceDbContext();
        var ledgerEntries = await financeContext.LedgerEntries
            .Where(e => e.ReferenceId == saleOrderId)
            .ToListAsync();
        Assert.Empty(ledgerEntries);

        var orderResponse = await _client.GetAsync($"/api/v1/sales/orders/{saleOrderId}");
        var order = await orderResponse.Content.ReadFromJsonAsync<SaleOrderDto>();
        Assert.Equal("Draft", order!.Status);
    }

    [Fact]
    public async Task ConfirmSaleOrder_WithSufficientStock_CommitsStockDecrementAndLedgerEntryTogether()
    {
        var product = await SeedProductAsync(initialQuantity: 10);
        var saleOrderId = await CreateSaleOrderAsync(product.Id, quantity: 4, unitPrice: 25m);

        var confirmResponse = await _client.PostAsync($"/api/v1/sales/orders/{saleOrderId}/confirm", null);

        Assert.Equal(HttpStatusCode.NoContent, confirmResponse.StatusCode);

        await using var inventoryContext = fixture.CreateInventoryDbContext();
        var persistedProduct = await new ProductRepository(inventoryContext)
            .GetByIdAsync(product.Id, CancellationToken.None);
        Assert.Equal(6, persistedProduct!.QuantityOnHand);

        await using var financeContext = fixture.CreateFinanceDbContext();
        var ledgerEntry = await financeContext.LedgerEntries
            .SingleAsync(e => e.ReferenceId == saleOrderId);
        Assert.Equal(100m, ledgerEntry.Amount); // 4 * 25

        var orderResponse = await _client.GetAsync($"/api/v1/sales/orders/{saleOrderId}");
        var order = await orderResponse.Content.ReadFromJsonAsync<SaleOrderDto>();
        Assert.Equal("Confirmed", order!.Status);
    }

    private record CreatedResponse(Guid Id);
}
