using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Domain.Products;

public record ProductLowStockEvent(Guid ProductId, string Sku, int QuantityOnHand, int ReorderThreshold)
    : IDomainEvent;
