using FluentValidation;

namespace SwiftERP.Inventory.Application.Products.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ReorderThreshold).GreaterThanOrEqualTo(0);
        RuleFor(x => x.InitialQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SupplierId).NotEmpty();
    }
}
