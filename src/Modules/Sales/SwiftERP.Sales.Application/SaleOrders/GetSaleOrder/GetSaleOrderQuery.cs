using MediatR;

namespace SwiftERP.Sales.Application.SaleOrders.GetSaleOrder;

public record GetSaleOrderQuery(Guid SaleOrderId) : IRequest<SaleOrderDto?>;
