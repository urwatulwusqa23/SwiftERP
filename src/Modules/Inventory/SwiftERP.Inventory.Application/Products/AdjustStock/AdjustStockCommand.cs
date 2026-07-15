using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Application.Products.AdjustStock;

public record AdjustStockCommand(Guid ProductId, int NewQuantity) : IRequest<Result>;
