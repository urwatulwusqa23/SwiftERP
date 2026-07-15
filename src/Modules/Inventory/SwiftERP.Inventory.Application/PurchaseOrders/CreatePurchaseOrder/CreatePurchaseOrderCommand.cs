using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Application.PurchaseOrders.CreatePurchaseOrder;

public record CreatePurchaseOrderLineDto(Guid ProductId, int Quantity, decimal UnitCost);

public record CreatePurchaseOrderCommand(
    Guid SupplierId,
    List<CreatePurchaseOrderLineDto> Lines) : IRequest<Result<Guid>>;
