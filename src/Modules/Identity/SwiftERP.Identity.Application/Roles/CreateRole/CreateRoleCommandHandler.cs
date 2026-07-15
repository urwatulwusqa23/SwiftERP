using MediatR;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.Identity.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Application.Roles.CreateRole;

public class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var existing = await roleRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>($"A role named '{request.Name}' already exists.");

        var role = new Role(request.Name);

        roleRepository.Add(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(role.Id);
    }
}
