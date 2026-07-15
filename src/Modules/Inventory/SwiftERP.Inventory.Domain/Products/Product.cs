using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Domain.Products;

public class Product : Entity
{
    public string Sku { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public int QuantityOnHand { get; private set; }
    public int ReorderThreshold { get; private set; }
    public Guid SupplierId { get; private set; }
    public byte[] RowVersion { get; private set; } = default!;

    private Product()
    {
    }

    public Product(string sku, string name, int reorderThreshold, Guid supplierId, int initialQuantity = 0)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("SKU cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty.");
        if (reorderThreshold < 0)
            throw new DomainException("Reorder threshold cannot be negative.");
        if (initialQuantity < 0)
            throw new DomainException("Initial quantity cannot be negative.");

        Sku = sku;
        Name = name;
        ReorderThreshold = reorderThreshold;
        SupplierId = supplierId;
        QuantityOnHand = initialQuantity;

        RaiseLowStockEventIfNeeded();
    }

    public void ReceiveStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Received quantity must be positive.");

        QuantityOnHand += quantity;
    }

    public void DecrementStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Decrement quantity must be positive.");
        if (quantity > QuantityOnHand)
            throw new DomainException(
                $"Cannot decrement {quantity} units of '{Sku}': only {QuantityOnHand} on hand.");

        QuantityOnHand -= quantity;

        RaiseLowStockEventIfNeeded();
    }

    public void AdjustStock(int newQuantity)
    {
        if (newQuantity < 0)
            throw new DomainException("Stock quantity cannot be negative.");

        QuantityOnHand = newQuantity;

        RaiseLowStockEventIfNeeded();
    }

    public bool IsLowStock => QuantityOnHand <= ReorderThreshold;

    private void RaiseLowStockEventIfNeeded()
    {
        if (IsLowStock)
            Raise(new ProductLowStockEvent(Id, Sku, QuantityOnHand, ReorderThreshold));
    }
}
