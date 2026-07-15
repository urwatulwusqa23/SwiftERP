using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Application.PurchaseOrders.ReceivePurchaseOrder;

public record ReceivePurchaseOrderCommand(Guid PurchaseOrderId) : IRequest<Result>;
