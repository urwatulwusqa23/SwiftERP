using SwiftERP.SharedKernel;

namespace SwiftERP.Sales.Domain.SaleOrders;

public enum SaleOrderStatus
{
    Draft,
    Confirmed,
    Cancelled
}

public enum PaymentStatus
{
    Unpaid,
    Paid
}

public class SaleOrder : Entity
{
    public Guid CustomerId { get; private set; }
    public SaleOrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ConfirmedAtUtc { get; private set; }

    private readonly List<SaleOrderLine> _lines = [];
    public IReadOnlyCollection<SaleOrderLine> Lines => _lines.AsReadOnly();

    public decimal Total => _lines.Sum(l => l.Quantity * l.UnitPrice);

    private SaleOrder()
    {
    }

    public SaleOrder(Guid customerId, IEnumerable<SaleOrderLine> lines)
    {
        var lineList = lines.ToList();
        if (lineList.Count == 0)
            throw new DomainException("A sale order must have at least one line.");

        CustomerId = customerId;
        Status = SaleOrderStatus.Draft;
        PaymentStatus = PaymentStatus.Unpaid;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        _lines.AddRange(lineList);
    }

    public void Confirm()
    {
        if (Status != SaleOrderStatus.Draft)
            throw new DomainException($"Cannot confirm a sale order in status '{Status}'.");

        Status = SaleOrderStatus.Confirmed;
        ConfirmedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == SaleOrderStatus.Confirmed)
            throw new DomainException("Cannot cancel a sale order that has already been confirmed.");

        Status = SaleOrderStatus.Cancelled;
    }

    public void MarkPaid()
    {
        if (Status != SaleOrderStatus.Confirmed)
            throw new DomainException("Only a confirmed sale order can be marked as paid.");
        if (PaymentStatus == PaymentStatus.Paid)
            throw new DomainException("Sale order is already marked as paid.");

        PaymentStatus = PaymentStatus.Paid;
    }
}

public class SaleOrderLine
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private SaleOrderLine()
    {
    }

    public SaleOrderLine(Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new DomainException("Sale order line quantity must be positive.");
        if (unitPrice < 0)
            throw new DomainException("Sale order line unit price cannot be negative.");

        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
