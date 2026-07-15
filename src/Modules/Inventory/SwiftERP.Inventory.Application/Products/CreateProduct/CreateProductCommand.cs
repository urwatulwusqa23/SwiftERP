using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Application.Products.CreateProduct;

public record CreateProductCommand(
    string Sku,
    string Name,
    int ReorderThreshold,
    Guid SupplierId,
    int InitialQuantity) : IRequest<Result<Guid>>;
