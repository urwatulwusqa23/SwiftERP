namespace SwiftERP.Inventory.Domain.PurchaseOrders;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(PurchaseOrder purchaseOrder);
}
