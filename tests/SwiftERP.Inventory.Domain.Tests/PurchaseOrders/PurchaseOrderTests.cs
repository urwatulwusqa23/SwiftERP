using SwiftERP.Inventory.Domain.PurchaseOrders;
using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Domain.Tests.PurchaseOrders;

public class PurchaseOrderTests
{
    private static List<PurchaseOrderLine> OneLine() =>
        [new PurchaseOrderLine(Guid.NewGuid(), 10, 5.50m)];

    [Fact]
    public void Constructor_WithNoLines_Throws()
    {
        Assert.Throws<DomainException>(() => new PurchaseOrder(Guid.NewGuid(), []));
    }

    [Fact]
    public void Constructor_WithValidLines_StartsPending()
    {
        var po = new PurchaseOrder(Guid.NewGuid(), OneLine());

        Assert.Equal(PurchaseOrderStatus.Pending, po.Status);
        Assert.Single(po.Lines);
    }

    [Fact]
    public void MarkReceived_WhenPending_TransitionsToReceived()
    {
        var po = new PurchaseOrder(Guid.NewGuid(), OneLine());

        po.MarkReceived();

        Assert.Equal(PurchaseOrderStatus.Received, po.Status);
        Assert.NotNull(po.ReceivedAtUtc);
    }

    [Fact]
    public void MarkReceived_WhenAlreadyReceived_Throws()
    {
        var po = new PurchaseOrder(Guid.NewGuid(), OneLine());
        po.MarkReceived();

        Assert.Throws<DomainException>(po.MarkReceived);
    }

    [Fact]
    public void Cancel_WhenPending_TransitionsToCancelled()
    {
        var po = new PurchaseOrder(Guid.NewGuid(), OneLine());

        po.Cancel();

        Assert.Equal(PurchaseOrderStatus.Cancelled, po.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyReceived_Throws()
    {
        var po = new PurchaseOrder(Guid.NewGuid(), OneLine());
        po.MarkReceived();

        Assert.Throws<DomainException>(po.Cancel);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PurchaseOrderLine_WithNonPositiveQuantity_Throws(int quantity)
    {
        Assert.Throws<DomainException>(() => new PurchaseOrderLine(Guid.NewGuid(), quantity, 1m));
    }

    [Fact]
    public void PurchaseOrderLine_WithNegativeUnitCost_Throws()
    {
        Assert.Throws<DomainException>(() => new PurchaseOrderLine(Guid.NewGuid(), 1, -0.01m));
    }
}
