using MediatR;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.Identity.Application.Abstractions;
using SwiftERP.Identity.Domain.Shared;
using SwiftERP.Identity.Domain.Users;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Application.Users.CreateUserAccount;

public class CreateUserAccountCommandHandler(
    IUserRepository userRepository,
    IEmployeeRepository employeeRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateUserAccountCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserAccountCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result.Failure<Guid>($"Employee '{request.EmployeeId}' was not found.");

        var existingForEmployee = await userRepository.GetByEmployeeIdAsync(request.EmployeeId, cancellationToken);
        if (existingForEmployee is not null)
            return Result.Failure<Guid>("This employee already has a user account.");

        var existingByEmail = await userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (existingByEmail is not null)
            return Result.Failure<Guid>($"Email '{request.Email}' is already in use.");

        var user = new User(request.EmployeeId, request.Email, passwordHasher.Hash(request.Password));
        foreach (var roleId in request.RoleIds)
            user.AssignRole(roleId);

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
