using SwiftERP.Identity.Application.Auth.Login;
using SwiftERP.Identity.Application.Tests.Fakes;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.Identity.Domain.Users;

namespace SwiftERP.Identity.Application.Tests;

public class LoginCommandHandlerTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeRoleRepository _roles = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeTokenIssuer _tokenIssuer = new();

    private LoginCommandHandler CreateHandler() => new(_users, _roles, _hasher, _tokenIssuer);

    private (User user, Guid employeeId) SeedUser(string email, string password, params Role[] roles)
    {
        var employeeId = Guid.NewGuid();
        var user = new User(employeeId, email, _hasher.Hash(password));
        foreach (var role in roles)
        {
            _roles.Seed(role);
            user.AssignRole(role.Id);
        }
        _users.Seed(user);
        return (user, employeeId);
    }

    [Fact]
    public async Task Handle_fails_for_unknown_email()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new LoginCommand("nobody@swifterp.local", "whatever"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_fails_for_wrong_password()
    {
        SeedUser("user@swifterp.local", "correct-password");
        var handler = CreateHandler();

        var result = await handler.Handle(new LoginCommand("user@swifterp.local", "wrong-password"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_fails_for_deactivated_user()
    {
        var (user, _) = SeedUser("user@swifterp.local", "password123");
        user.Deactivate();
        var handler = CreateHandler();

        var result = await handler.Handle(new LoginCommand("user@swifterp.local", "password123"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_succeeds_and_issues_the_max_permission_across_all_assigned_roles()
    {
        var salesRole = new Role("Sales Rep");
        salesRole.SetPermission(Module.Sales, AccessLevel.Full);
        salesRole.SetPermission(Module.Inventory, AccessLevel.View);

        var warehouseRole = new Role("Warehouse Staff");
        warehouseRole.SetPermission(Module.Inventory, AccessLevel.Full);

        SeedUser("dual-role@swifterp.local", "password123", salesRole, warehouseRole);
        var handler = CreateHandler();

        var result = await handler.Handle(new LoginCommand("dual-role@swifterp.local", "password123"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        // Inventory: View (from Sales Rep) and Full (from Warehouse Staff) -> the higher wins.
        Assert.Equal(AccessLevel.Full, _tokenIssuer.LastPermissions![Module.Inventory]);
        Assert.Equal(AccessLevel.Full, _tokenIssuer.LastPermissions![Module.Sales]);
        Assert.Equal(AccessLevel.None, _tokenIssuer.LastPermissions![Module.Finance]);
    }

    [Fact]
    public async Task Handle_grants_no_permissions_when_the_user_has_no_roles()
    {
        SeedUser("no-roles@swifterp.local", "password123");
        var handler = CreateHandler();

        var result = await handler.Handle(new LoginCommand("no-roles@swifterp.local", "password123"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.All(Enum.GetValues<Module>(), module => Assert.Equal(AccessLevel.None, _tokenIssuer.LastPermissions![module]));
    }

    [Fact]
    public async Task Handle_sets_isSystemAdmin_true_only_for_a_role_actually_named_Admin()
    {
        var adminRole = new Role("Admin", isSystemRole: true);
        SeedUser("admin@swifterp.local", "password123", adminRole);
        var handler = CreateHandler();

        await handler.Handle(new LoginCommand("admin@swifterp.local", "password123"), CancellationToken.None);

        Assert.True(_tokenIssuer.LastIsSystemAdmin);
    }

    [Fact]
    public async Task Handle_does_not_grant_isSystemAdmin_to_other_system_roles()
    {
        // Regression test for the privilege-escalation bug: the seeded "Employee" role is also
        // IsSystemRole=true (protected from rename), but must never grant admin access. The
        // login handler must key isSystemAdmin off the role's *name*, not the IsSystemRole flag.
        var employeeRole = new Role("Employee", isSystemRole: true);
        employeeRole.SetPermission(Module.Inventory, AccessLevel.View);
        SeedUser("employee@swifterp.local", "password123", employeeRole);
        var handler = CreateHandler();

        await handler.Handle(new LoginCommand("employee@swifterp.local", "password123"), CancellationToken.None);

        Assert.False(_tokenIssuer.LastIsSystemAdmin);
    }
}
