using MediatR;
using SwiftERP.Sales.Domain.SaleOrders;

namespace SwiftERP.Sales.Application.SaleOrders.GetSaleOrder;

public class GetSaleOrderQueryHandler(ISaleOrderRepository saleOrderRepository)
    : IRequestHandler<GetSaleOrderQuery, SaleOrderDto?>
{
    public async Task<SaleOrderDto?> Handle(GetSaleOrderQuery request, CancellationToken cancellationToken)
    {
        var saleOrder = await saleOrderRepository.GetByIdAsync(request.SaleOrderId, cancellationToken);
        if (saleOrder is null)
            return null;

        return new SaleOrderDto(
            saleOrder.Id,
            saleOrder.CustomerId,
            saleOrder.Status.ToString(),
            saleOrder.PaymentStatus.ToString(),
            saleOrder.Total,
            saleOrder.Lines.Select(l => new SaleOrderLineDto(l.ProductId, l.Quantity, l.UnitPrice)).ToList());
    }
}
