using MediatR;
using SwiftERP.Inventory.Domain.Products;
using SwiftERP.Inventory.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Application.Products.CreateProduct;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existing = await productRepository.GetBySkuAsync(request.Sku, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>($"A product with SKU '{request.Sku}' already exists.");

        var product = new Product(
            request.Sku,
            request.Name,
            request.ReorderThreshold,
            request.SupplierId,
            request.InitialQuantity);

        productRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}
