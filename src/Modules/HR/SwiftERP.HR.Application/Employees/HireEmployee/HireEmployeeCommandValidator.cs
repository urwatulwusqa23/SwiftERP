using FluentValidation;

namespace SwiftERP.HR.Application.Employees.HireEmployee;

public class HireEmployeeCommandValidator : AbstractValidator<HireEmployeeCommand>
{
    public HireEmployeeCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.MonthlySalary).GreaterThan(0);
    }
}
