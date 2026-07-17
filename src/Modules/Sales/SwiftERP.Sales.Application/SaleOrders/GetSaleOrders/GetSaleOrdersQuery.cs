using MediatR;
using SwiftERP.Sales.Application.SaleOrders.GetSaleOrder;

namespace SwiftERP.Sales.Application.SaleOrders.GetSaleOrders;

public record GetSaleOrdersQuery : IRequest<List<SaleOrderDto>>;
