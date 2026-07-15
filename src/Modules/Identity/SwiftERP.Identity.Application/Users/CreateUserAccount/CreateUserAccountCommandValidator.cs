using FluentValidation;

namespace SwiftERP.Identity.Application.Users.CreateUserAccount;

public class CreateUserAccountCommandValidator : AbstractValidator<CreateUserAccountCommand>
{
    public CreateUserAccountCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.");
    }
}
