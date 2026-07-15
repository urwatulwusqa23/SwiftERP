using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Domain.PurchaseOrders;

public enum PurchaseOrderStatus
{
    Pending,
    Received,
    Cancelled
}

public class PurchaseOrder : Entity
{
    public Guid SupplierId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ReceivedAtUtc { get; private set; }

    private readonly List<PurchaseOrderLine> _lines = [];
    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    private PurchaseOrder()
    {
    }

    public PurchaseOrder(Guid supplierId, IEnumerable<PurchaseOrderLine> lines)
    {
        var lineList = lines.ToList();
        if (lineList.Count == 0)
            throw new DomainException("A purchase order must have at least one line.");

        SupplierId = supplierId;
        Status = PurchaseOrderStatus.Pending;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        _lines.AddRange(lineList);
    }

    public void MarkReceived()
    {
        if (Status != PurchaseOrderStatus.Pending)
            throw new DomainException($"Cannot receive a purchase order in status '{Status}'.");

        Status = PurchaseOrderStatus.Received;
        ReceivedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status != PurchaseOrderStatus.Pending)
            throw new DomainException($"Cannot cancel a purchase order in status '{Status}'.");

        Status = PurchaseOrderStatus.Cancelled;
    }
}

public class PurchaseOrderLine
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitCost { get; private set; }

    private PurchaseOrderLine()
    {
    }

    public PurchaseOrderLine(Guid productId, int quantity, decimal unitCost)
    {
        if (quantity <= 0)
            throw new DomainException("Purchase order line quantity must be positive.");
        if (unitCost < 0)
            throw new DomainException("Purchase order line unit cost cannot be negative.");

        ProductId = productId;
        Quantity = quantity;
        UnitCost = unitCost;
    }
}
