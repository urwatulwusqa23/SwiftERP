using MediatR;
using SwiftERP.Inventory.Domain.Products;
using SwiftERP.Inventory.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Application.Products.AdjustStock;

public class AdjustStockCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AdjustStockCommand, Result>
{
    public async Task<Result> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure($"Product '{request.ProductId}' was not found.");

        product.AdjustStock(request.NewQuantity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
