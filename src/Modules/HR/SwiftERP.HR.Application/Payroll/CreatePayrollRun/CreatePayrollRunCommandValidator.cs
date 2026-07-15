using FluentValidation;

namespace SwiftERP.HR.Application.Payroll.CreatePayrollRun;

public class CreatePayrollRunCommandValidator : AbstractValidator<CreatePayrollRunCommand>
{
    public CreatePayrollRunCommandValidator()
    {
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).GreaterThan(2000);
    }
}
