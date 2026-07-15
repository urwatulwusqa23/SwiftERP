using Microsoft.EntityFrameworkCore;
using SwiftERP.Inventory.Domain.Products;
using SwiftERP.Inventory.Infrastructure.Persistence.Repositories;
using SwiftERP.Inventory.Integration.Tests.Infrastructure;

namespace SwiftERP.Inventory.Integration.Tests.Products;

[Collection(SqlServerCollection.Name)]
public class ProductRepositoryTests(SqlServerContainerFixture fixture)
{
    [Fact]
    public async Task AddAndSave_PersistsProduct_RetrievableBySku()
    {
        var supplierId = Guid.NewGuid();
        var product = new Product($"SKU-{Guid.NewGuid():N}", "Widget", reorderThreshold: 5, supplierId, initialQuantity: 50);

        await using (var dbContext = fixture.CreateDbContext())
        {
            var repository = new ProductRepository(dbContext);
            repository.Add(product);
            await dbContext.SaveChangesAsync();
        }

        await using (var readContext = fixture.CreateDbContext())
        {
            var repository = new ProductRepository(readContext);
            var persisted = await repository.GetBySkuAsync(product.Sku, CancellationToken.None);

            Assert.NotNull(persisted);
            Assert.Equal(50, persisted!.QuantityOnHand);
        }
    }

    [Fact]
    public async Task GetLowStockAsync_ReturnsOnlyProductsAtOrBelowThreshold()
    {
        var supplierId = Guid.NewGuid();
        var lowStockProduct = new Product($"SKU-{Guid.NewGuid():N}", "Low Stock Widget", reorderThreshold: 10, supplierId, initialQuantity: 5);
        var healthyProduct = new Product($"SKU-{Guid.NewGuid():N}", "Healthy Widget", reorderThreshold: 10, supplierId, initialQuantity: 100);

        await using (var dbContext = fixture.CreateDbContext())
        {
            var repository = new ProductRepository(dbContext);
            repository.Add(lowStockProduct);
            repository.Add(healthyProduct);
            await dbContext.SaveChangesAsync();
        }

        await using var readContext = fixture.CreateDbContext();
        var readRepository = new ProductRepository(readContext);
        var lowStock = await readRepository.GetLowStockAsync(CancellationToken.None);

        Assert.Contains(lowStock, p => p.Id == lowStockProduct.Id);
        Assert.DoesNotContain(lowStock, p => p.Id == healthyProduct.Id);
    }

    [Fact]
    public async Task DecrementStock_ConcurrentDecrementsExceedingStock_DoesNotOversell()
    {
        var supplierId = Guid.NewGuid();
        var product = new Product($"SKU-{Guid.NewGuid():N}", "Scarce Widget", reorderThreshold: 0, supplierId, initialQuantity: 10);

        await using (var setupContext = fixture.CreateDbContext())
        {
            var repository = new ProductRepository(setupContext);
            repository.Add(product);
            await setupContext.SaveChangesAsync();
        }

        // Two concurrent "sales" each try to decrement 8 units against only 10 in stock.
        // Both contexts load the product (and its RowVersion) before either one saves,
        // simulating two requests that read stale-but-plausible state at the same time.
        // Only one save may win — the loser must hit a concurrency conflict rather than
        // silently overselling.
        await using var dbContextA = fixture.CreateDbContext();
        await using var dbContextB = fixture.CreateDbContext();

        var productA = await new ProductRepository(dbContextA).GetByIdAsync(product.Id, CancellationToken.None);
        var productB = await new ProductRepository(dbContextB).GetByIdAsync(product.Id, CancellationToken.None);

        productA!.DecrementStock(8);
        productB!.DecrementStock(8);

        async Task<bool> TrySave(DbContext dbContext)
        {
            try
            {
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        var results = await Task.WhenAll(TrySave(dbContextA), TrySave(dbContextB));

        Assert.Single(results, success => success);
        Assert.Single(results, success => !success);

        await using var finalContext = fixture.CreateDbContext();
        var finalRepository = new ProductRepository(finalContext);
        var finalProduct = await finalRepository.GetByIdAsync(product.Id, CancellationToken.None);

        Assert.Equal(2, finalProduct!.QuantityOnHand);
        Assert.True(finalProduct.QuantityOnHand >= 0);
    }
}
