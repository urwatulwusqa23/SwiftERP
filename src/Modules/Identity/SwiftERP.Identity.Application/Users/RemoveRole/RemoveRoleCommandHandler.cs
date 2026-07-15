using MediatR;
using SwiftERP.Identity.Domain.Shared;
using SwiftERP.Identity.Domain.Users;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Application.Users.RemoveRole;

public class RemoveRoleCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoveRoleCommand, Result>
{
    public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure($"User '{request.UserId}' was not found.");

        user.RemoveRole(request.RoleId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
