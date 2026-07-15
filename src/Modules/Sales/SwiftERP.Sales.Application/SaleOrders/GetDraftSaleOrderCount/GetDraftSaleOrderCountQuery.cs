using MediatR;

namespace SwiftERP.Sales.Application.SaleOrders.GetDraftSaleOrderCount;

public record GetDraftSaleOrderCountQuery : IRequest<int>;
