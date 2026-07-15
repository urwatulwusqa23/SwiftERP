using SwiftERP.Inventory.Domain.Products;

namespace SwiftERP.Sales.Application.Abstractions;

/// <summary>
/// Sales-owned port onto the Product aggregate for the ConfirmSaleOrder workflow. Deliberately a
/// distinct interface from Inventory's own <see cref="IProductRepository"/> — both are implemented
/// against the Products table, but keeping them as separate DI registrations avoids one module's
/// Infrastructure registration silently shadowing the other's in the shared container.
/// </summary>
public interface ISalesInventoryPort
{
    Task<Product?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken);
}
