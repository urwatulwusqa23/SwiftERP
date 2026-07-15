using MediatR;

namespace SwiftERP.Inventory.Application.Products.GetLowStockProducts;

public record GetLowStockProductsQuery : IRequest<List<LowStockProductDto>>;
