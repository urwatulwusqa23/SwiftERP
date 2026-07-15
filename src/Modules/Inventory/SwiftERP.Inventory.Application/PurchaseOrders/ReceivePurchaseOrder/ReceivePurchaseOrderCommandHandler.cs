using MediatR;
using SwiftERP.Inventory.Domain.Products;
using SwiftERP.Inventory.Domain.PurchaseOrders;
using SwiftERP.Inventory.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Application.PurchaseOrders.ReceivePurchaseOrder;

public class ReceivePurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ReceivePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(ReceivePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId, cancellationToken);
        if (purchaseOrder is null)
            return Result.Failure($"Purchase order '{request.PurchaseOrderId}' was not found.");

        foreach (var line in purchaseOrder.Lines)
        {
            var product = await productRepository.GetByIdAsync(line.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure($"Product '{line.ProductId}' referenced by purchase order line was not found.");

            product.ReceiveStock(line.Quantity);
        }

        purchaseOrder.MarkReceived();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
