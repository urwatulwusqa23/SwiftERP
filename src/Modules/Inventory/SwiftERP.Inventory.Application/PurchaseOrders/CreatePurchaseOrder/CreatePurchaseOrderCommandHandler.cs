using MediatR;
using SwiftERP.Inventory.Domain.PurchaseOrders;
using SwiftERP.Inventory.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Application.PurchaseOrders.CreatePurchaseOrder;

public class CreatePurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreatePurchaseOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var lines = request.Lines
            .Select(l => new PurchaseOrderLine(l.ProductId, l.Quantity, l.UnitCost))
            .ToList();

        var purchaseOrder = new PurchaseOrder(request.SupplierId, lines);

        purchaseOrderRepository.Add(purchaseOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(purchaseOrder.Id);
    }
}
