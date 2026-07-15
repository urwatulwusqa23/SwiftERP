using MediatR;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.Identity.Domain.Shared;
using SwiftERP.Identity.Domain.Users;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Application.Users.AssignRole;

public class AssignRoleCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignRoleCommand, Result>
{
    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure($"User '{request.UserId}' was not found.");

        var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
            return Result.Failure($"Role '{request.RoleId}' was not found.");

        user.AssignRole(role.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
