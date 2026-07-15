using MediatR;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Application.Roles.UpdateRolePermission;

public record UpdateRolePermissionCommand(Guid RoleId, Module Module, AccessLevel AccessLevel) : IRequest<Result>;
