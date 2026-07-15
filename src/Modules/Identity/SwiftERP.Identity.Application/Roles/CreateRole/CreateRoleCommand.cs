using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Application.Roles.CreateRole;

public record CreateRoleCommand(string Name) : IRequest<Result<Guid>>;
