using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.Sales.Application.SaleOrders.ConfirmSaleOrder;

public record ConfirmSaleOrderCommand(Guid SaleOrderId) : IRequest<Result>;
