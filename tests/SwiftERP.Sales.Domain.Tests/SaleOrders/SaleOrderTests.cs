using SwiftERP.Sales.Domain.SaleOrders;
using SwiftERP.SharedKernel;

namespace SwiftERP.Sales.Domain.Tests.SaleOrders;

public class SaleOrderTests
{
    private static List<SaleOrderLine> TwoLines() =>
    [
        new SaleOrderLine(Guid.NewGuid(), 2, 10m),
        new SaleOrderLine(Guid.NewGuid(), 3, 5m)
    ];

    [Fact]
    public void Constructor_WithNoLines_Throws()
    {
        Assert.Throws<DomainException>(() => new SaleOrder(Guid.NewGuid(), []));
    }

    [Fact]
    public void Constructor_WithValidLines_StartsDraftAndUnpaid()
    {
        var order = new SaleOrder(Guid.NewGuid(), TwoLines());

        Assert.Equal(SaleOrderStatus.Draft, order.Status);
        Assert.Equal(PaymentStatus.Unpaid, order.PaymentStatus);
    }

    [Fact]
    public void Total_SumsQuantityTimesUnitPriceAcrossLines()
    {
        var order = new SaleOrder(Guid.NewGuid(), TwoLines());

        // (2 * 10) + (3 * 5) = 35
        Assert.Equal(35m, order.Total);
    }

    [Fact]
    public void Confirm_WhenDraft_TransitionsToConfirmed()
    {
        var order = new SaleOrder(Guid.NewGuid(), TwoLines());

        order.Confirm();

        Assert.Equal(SaleOrderStatus.Confirmed, order.Status);
        Assert.NotNull(order.ConfirmedAtUtc);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_Throws()
    {
        var order = new SaleOrder(Guid.NewGuid(), TwoLines());
        order.Confirm();

        Assert.Throws<DomainException>(order.Confirm);
    }

    [Fact]
    public void Cancel_WhenConfirmed_Throws()
    {
        var order = new SaleOrder(Guid.NewGuid(), TwoLines());
        order.Confirm();

        Assert.Throws<DomainException>(order.Cancel);
    }

    [Fact]
    public void Cancel_WhenDraft_TransitionsToCancelled()
    {
        var order = new SaleOrder(Guid.NewGuid(), TwoLines());

        order.Cancel();

        Assert.Equal(SaleOrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void MarkPaid_WhenConfirmed_SetsPaymentStatusPaid()
    {
        var order = new SaleOrder(Guid.NewGuid(), TwoLines());
        order.Confirm();

        order.MarkPaid();

        Assert.Equal(PaymentStatus.Paid, order.PaymentStatus);
    }

    [Fact]
    public void MarkPaid_WhenStillDraft_Throws()
    {
        var order = new SaleOrder(Guid.NewGuid(), TwoLines());

        Assert.Throws<DomainException>(order.MarkPaid);
    }

    [Fact]
    public void MarkPaid_WhenAlreadyPaid_Throws()
    {
        var order = new SaleOrder(Guid.NewGuid(), TwoLines());
        order.Confirm();
        order.MarkPaid();

        Assert.Throws<DomainException>(order.MarkPaid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SaleOrderLine_WithNonPositiveQuantity_Throws(int quantity)
    {
        Assert.Throws<DomainException>(() => new SaleOrderLine(Guid.NewGuid(), quantity, 1m));
    }

    [Fact]
    public void SaleOrderLine_WithNegativeUnitPrice_Throws()
    {
        Assert.Throws<DomainException>(() => new SaleOrderLine(Guid.NewGuid(), 1, -0.01m));
    }
}
