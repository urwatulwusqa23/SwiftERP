using Microsoft.EntityFrameworkCore;
using SwiftERP.Inventory.Domain.Products;

namespace SwiftERP.Inventory.Infrastructure.Persistence.Repositories;

public class ProductRepository(InventoryDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken) =>
        dbContext.Products.FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);

    public Task<List<Product>> GetLowStockAsync(CancellationToken cancellationToken) =>
        dbContext.Products
            .Where(p => p.QuantityOnHand <= p.ReorderThreshold)
            .ToListAsync(cancellationToken);

    public void Add(Product product) => dbContext.Products.Add(product);
}
