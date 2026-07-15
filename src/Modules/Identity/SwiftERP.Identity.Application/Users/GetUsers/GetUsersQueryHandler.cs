using MediatR;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.Identity.Domain.Users;

namespace SwiftERP.Identity.Application.Users.GetUsers;

public class GetUsersQueryHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IEmployeeRepository employeeRepository) : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        var roles = await roleRepository.GetAllAsync(cancellationToken);
        var roleNamesById = roles.ToDictionary(r => r.Id, r => r.Name);

        var result = new List<UserDto>();
        foreach (var user in users)
        {
            var employee = await employeeRepository.GetByIdAsync(user.EmployeeId, cancellationToken);
            var roleIds = user.Roles.Select(r => r.RoleId).ToList();

            result.Add(new UserDto(
                user.Id,
                user.EmployeeId,
                employee?.FullName ?? "(unknown employee)",
                user.Email,
                user.IsActive,
                roleIds,
                roleIds.Select(id => roleNamesById.GetValueOrDefault(id, "(unknown role)")).ToList()));
        }

        return result;
    }
}
