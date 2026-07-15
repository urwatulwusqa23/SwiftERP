using Microsoft.EntityFrameworkCore;
using SwiftERP.Inventory.Domain.PurchaseOrders;

namespace SwiftERP.Inventory.Infrastructure.Persistence.Repositories;

public class PurchaseOrderRepository(InventoryDbContext dbContext) : IPurchaseOrderRepository
{
    public Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.PurchaseOrders
            .Include(po => po.Lines)
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);

    public void Add(PurchaseOrder purchaseOrder) => dbContext.PurchaseOrders.Add(purchaseOrder);
}
