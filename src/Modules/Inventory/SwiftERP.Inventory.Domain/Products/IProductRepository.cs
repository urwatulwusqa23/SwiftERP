namespace SwiftERP.Inventory.Domain.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken);
    Task<List<Product>> GetLowStockAsync(CancellationToken cancellationToken);
    void Add(Product product);
}
