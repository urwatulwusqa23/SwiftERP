using MediatR;
using SwiftERP.Sales.Application.SaleOrders.GetSaleOrder;
using SwiftERP.Sales.Domain.SaleOrders;

namespace SwiftERP.Sales.Application.SaleOrders.GetSaleOrders;

public class GetSaleOrdersQueryHandler(ISaleOrderRepository saleOrderRepository)
    : IRequestHandler<GetSaleOrdersQuery, List<SaleOrderDto>>
{
    public async Task<List<SaleOrderDto>> Handle(GetSaleOrdersQuery request, CancellationToken cancellationToken)
    {
        var saleOrders = await saleOrderRepository.GetAllAsync(cancellationToken);

        return saleOrders.Select(saleOrder => new SaleOrderDto(
            saleOrder.Id,
            saleOrder.CustomerId,
            saleOrder.Status.ToString(),
            saleOrder.PaymentStatus.ToString(),
            saleOrder.Total,
            saleOrder.Lines.Select(l => new SaleOrderLineDto(l.ProductId, l.Quantity, l.UnitPrice)).ToList()))
            .ToList();
    }
}
