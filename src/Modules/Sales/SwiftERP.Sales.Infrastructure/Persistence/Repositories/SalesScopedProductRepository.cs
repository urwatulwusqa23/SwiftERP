using Microsoft.EntityFrameworkCore;
using SwiftERP.Inventory.Domain.Products;
using SwiftERP.Sales.Application.Abstractions;

namespace SwiftERP.Sales.Infrastructure.Persistence.Repositories;

/// <summary>
/// Reads/writes the same Products table as Inventory's own repository, but through SalesDbContext
/// so that a stock decrement made during ConfirmSaleOrder shares the same change tracker and
/// transaction as the SaleOrder and LedgerEntry writes in that same handler.
/// </summary>
public class SalesScopedProductRepository(SalesDbContext dbContext) : ISalesInventoryPort
{
    public Task<Product?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken) =>
        dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
}
