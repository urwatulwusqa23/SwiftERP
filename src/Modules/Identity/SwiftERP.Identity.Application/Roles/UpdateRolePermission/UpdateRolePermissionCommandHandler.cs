using MediatR;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.Identity.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Application.Roles.UpdateRolePermission;

public class UpdateRolePermissionCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateRolePermissionCommand, Result>
{
    public async Task<Result> Handle(UpdateRolePermissionCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
            return Result.Failure($"Role '{request.RoleId}' was not found.");

        role.SetPermission(request.Module, request.AccessLevel);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
