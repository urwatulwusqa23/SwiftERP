using SwiftERP.Identity.Domain.Users;
using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Domain.Tests;

public class UserTests
{
    private static User CreateUser() => new(Guid.NewGuid(), " Admin@SwiftERP.Local ", "hash");

    [Fact]
    public void Constructor_normalizes_email_to_lowercase_and_trims()
    {
        var user = CreateUser();

        Assert.Equal("admin@swifterp.local", user.Email);
    }

    [Fact]
    public void New_user_is_active_by_default()
    {
        var user = CreateUser();

        Assert.True(user.IsActive);
    }

    [Fact]
    public void AssignRole_is_idempotent()
    {
        var user = CreateUser();
        var roleId = Guid.NewGuid();

        user.AssignRole(roleId);
        user.AssignRole(roleId);

        Assert.Single(user.Roles);
    }

    [Fact]
    public void RemoveRole_removes_only_the_matching_assignment()
    {
        var user = CreateUser();
        var roleA = Guid.NewGuid();
        var roleB = Guid.NewGuid();
        user.AssignRole(roleA);
        user.AssignRole(roleB);

        user.RemoveRole(roleA);

        Assert.DoesNotContain(user.Roles, r => r.RoleId == roleA);
        Assert.Contains(user.Roles, r => r.RoleId == roleB);
    }

    [Fact]
    public void Deactivate_then_Activate_round_trips()
    {
        var user = CreateUser();

        user.Deactivate();
        Assert.False(user.IsActive);

        user.Activate();
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Constructor_throws_for_empty_email_or_hash()
    {
        Assert.Throws<DomainException>(() => new User(Guid.NewGuid(), "", "hash"));
        Assert.Throws<DomainException>(() => new User(Guid.NewGuid(), "a@b.com", ""));
    }
}
