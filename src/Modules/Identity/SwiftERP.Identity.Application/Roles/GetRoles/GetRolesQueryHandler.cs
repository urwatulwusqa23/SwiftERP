using MediatR;
using SwiftERP.Identity.Domain.Roles;

namespace SwiftERP.Identity.Application.Roles.GetRoles;

public class GetRolesQueryHandler(IRoleRepository roleRepository) : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken);

        return roles
            .Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.IsSystemRole,
                r.Permissions.Select(p => new ModulePermissionDto(p.Module.ToString(), p.AccessLevel.ToString())).ToList()))
            .OrderBy(r => r.Name)
            .ToList();
    }
}
