using FluentValidation;

namespace SwiftERP.Sales.Application.SaleOrders.CreateSaleOrder;

public class CreateSaleOrderCommandValidator : AbstractValidator<CreateSaleOrderCommand>
{
    public CreateSaleOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
