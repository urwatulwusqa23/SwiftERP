using MediatR;
using SwiftERP.Inventory.Domain.Products;

namespace SwiftERP.Inventory.Application.Products.GetLowStockProducts;

public class GetLowStockProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetLowStockProductsQuery, List<LowStockProductDto>>
{
    public async Task<List<LowStockProductDto>> Handle(
        GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetLowStockAsync(cancellationToken);

        return products
            .Select(p => new LowStockProductDto(p.Id, p.Sku, p.Name, p.QuantityOnHand, p.ReorderThreshold))
            .ToList();
    }
}
