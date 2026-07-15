using MediatR;
using SwiftERP.Sales.Domain.SaleOrders;

namespace SwiftERP.Sales.Application.SaleOrders.GetDraftSaleOrderCount;

public class GetDraftSaleOrderCountQueryHandler(ISaleOrderRepository saleOrderRepository)
    : IRequestHandler<GetDraftSaleOrderCountQuery, int>
{
    public Task<int> Handle(GetDraftSaleOrderCountQuery request, CancellationToken cancellationToken) =>
        saleOrderRepository.GetDraftCountAsync(cancellationToken);
}
