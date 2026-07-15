using MediatR;
using SwiftERP.Sales.Domain.SaleOrders;
using SwiftERP.Sales.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.Sales.Application.SaleOrders.CreateSaleOrder;

public class CreateSaleOrderCommandHandler(
    ISaleOrderRepository saleOrderRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateSaleOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSaleOrderCommand request, CancellationToken cancellationToken)
    {
        var lines = request.Lines
            .Select(l => new SaleOrderLine(l.ProductId, l.Quantity, l.UnitPrice))
            .ToList();

        var saleOrder = new SaleOrder(request.CustomerId, lines);

        saleOrderRepository.Add(saleOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(saleOrder.Id);
    }
}
