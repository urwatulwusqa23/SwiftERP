using MediatR;
using SwiftERP.Identity.Application.Abstractions;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.Identity.Domain.Users;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Application.Auth.Login;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokenIssuer) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<LoginResponse>("Invalid email or password.");

        if (!user.IsActive)
            return Result.Failure<LoginResponse>("This account has been deactivated.");

        var allRoles = await roleRepository.GetAllAsync(cancellationToken);
        var userRoles = allRoles.Where(r => user.Roles.Any(ur => ur.RoleId == r.Id)).ToList();

        // Effective permission per module is the highest level granted by any of the user's
        // roles — holding multiple roles only ever grants more access, never less.
        var permissions = Enum.GetValues<Module>().ToDictionary(
            module => module,
            module => userRoles.Count == 0
                ? AccessLevel.None
                : userRoles.Max(r => r.GetPermission(module)));

        // IsSystemRole only means "protected from rename/deletion" — both the built-in "Admin"
        // and "Employee" roles carry it. Whether a role grants access to the admin endpoints is
        // a distinct question, decided by name rather than that flag, so seeding an unprivileged
        // system role (like "Employee") can never accidentally grant admin access.
        var isSystemAdmin = userRoles.Any(r => r.IsSystemRole && r.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase));
        var issued = tokenIssuer.Issue(user.Id, user.EmployeeId, user.Email, permissions, isSystemAdmin);

        var response = new LoginResponse(
            issued.Token,
            issued.ExpiresAtUtc,
            user.Id,
            user.EmployeeId,
            user.Email,
            userRoles.Select(r => r.Name).ToList(),
            permissions.ToDictionary(p => p.Key.ToString(), p => p.Value.ToString()));

        return Result.Success(response);
    }
}
