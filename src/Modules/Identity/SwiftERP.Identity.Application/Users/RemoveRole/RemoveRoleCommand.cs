using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Application.Users.RemoveRole;

public record RemoveRoleCommand(Guid UserId, Guid RoleId) : IRequest<Result>;
