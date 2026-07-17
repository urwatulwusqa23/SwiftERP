using SwiftERP.Identity.Domain.Roles;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Domain.Tests;

public class RoleTests
{
    [Fact]
    public void New_role_starts_with_None_on_every_module()
    {
        var role = new Role("Employee");

        foreach (var module in Enum.GetValues<Module>())
            Assert.Equal(AccessLevel.None, role.GetPermission(module));
    }

    [Fact]
    public void SetPermission_updates_the_level_for_that_module_only()
    {
        var role = new Role("Warehouse Staff");

        role.SetPermission(Module.Inventory, AccessLevel.Full);

        Assert.Equal(AccessLevel.Full, role.GetPermission(Module.Inventory));
        Assert.Equal(AccessLevel.None, role.GetPermission(Module.Finance));
    }

    [Fact]
    public void IsSystemRole_does_not_by_itself_imply_admin_access()
    {
        // Regression test: a real bug shipped where any role with IsSystemRole=true (including
        // the seeded "Employee" role) was treated as granting admin access, because the login
        // handler originally checked IsSystemRole instead of the role's name. IsSystemRole only
        // means "protected from rename/deletion" — it says nothing about permission level.
        var employeeRole = new Role("Employee", isSystemRole: true);

        Assert.True(employeeRole.IsSystemRole);
        Assert.Equal(AccessLevel.None, employeeRole.GetPermission(Module.Finance));
        Assert.NotEqual("Admin", employeeRole.Name);
    }

    [Fact]
    public void Rename_throws_for_system_roles()
    {
        var role = new Role("Admin", isSystemRole: true);

        Assert.Throws<DomainException>(() => role.Rename("Superuser"));
    }

    [Fact]
    public void Rename_succeeds_for_non_system_roles()
    {
        var role = new Role("Sales Rep");

        role.Rename("Senior Sales Rep");

        Assert.Equal("Senior Sales Rep", role.Name);
    }

    [Fact]
    public void Constructor_throws_for_empty_name()
    {
        Assert.Throws<DomainException>(() => new Role(""));
        Assert.Throws<DomainException>(() => new Role("   "));
    }
}
