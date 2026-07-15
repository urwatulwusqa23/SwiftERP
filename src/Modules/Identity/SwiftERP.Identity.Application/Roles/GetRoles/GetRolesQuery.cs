using MediatR;

namespace SwiftERP.Identity.Application.Roles.GetRoles;

public record GetRolesQuery : IRequest<List<RoleDto>>;
