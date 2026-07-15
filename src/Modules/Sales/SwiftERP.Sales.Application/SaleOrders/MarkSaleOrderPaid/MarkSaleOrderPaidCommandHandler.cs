using MediatR;
using SwiftERP.Sales.Domain.SaleOrders;
using SwiftERP.Sales.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.Sales.Application.SaleOrders.MarkSaleOrderPaid;

public class MarkSaleOrderPaidCommandHandler(
    ISaleOrderRepository saleOrderRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkSaleOrderPaidCommand, Result>
{
    public async Task<Result> Handle(MarkSaleOrderPaidCommand request, CancellationToken cancellationToken)
    {
        var saleOrder = await saleOrderRepository.GetByIdAsync(request.SaleOrderId, cancellationToken);
        if (saleOrder is null)
            return Result.Failure($"Sale order '{request.SaleOrderId}' was not found.");

        saleOrder.MarkPaid();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
