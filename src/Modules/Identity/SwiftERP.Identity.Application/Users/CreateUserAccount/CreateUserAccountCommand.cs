using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Application.Users.CreateUserAccount;

public record CreateUserAccountCommand(
    Guid EmployeeId,
    string Email,
    string Password,
    List<Guid> RoleIds) : IRequest<Result<Guid>>;
