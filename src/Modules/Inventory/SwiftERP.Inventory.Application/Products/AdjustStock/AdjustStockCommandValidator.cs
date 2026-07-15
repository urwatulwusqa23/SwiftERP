using FluentValidation;

namespace SwiftERP.Inventory.Application.Products.AdjustStock;

public class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0);
    }
}
