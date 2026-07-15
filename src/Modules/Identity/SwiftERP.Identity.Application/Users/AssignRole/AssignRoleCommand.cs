using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Application.Users.AssignRole;

public record AssignRoleCommand(Guid UserId, Guid RoleId) : IRequest<Result>;
