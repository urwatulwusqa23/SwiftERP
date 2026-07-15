using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.Sales.Application.SaleOrders.MarkSaleOrderPaid;

public record MarkSaleOrderPaidCommand(Guid SaleOrderId) : IRequest<Result>;
