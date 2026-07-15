using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Domain.Users;

public class User : Entity
{
    public Guid EmployeeId { get; private set; }
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public bool IsActive { get; private set; }

    private readonly List<UserRoleAssignment> _roles = [];
    public IReadOnlyCollection<UserRoleAssignment> Roles => _roles.AsReadOnly();

    private User()
    {
    }

    public User(Guid employeeId, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash cannot be empty.");

        EmployeeId = employeeId;
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        IsActive = true;
    }

    public void ChangePasswordHash(string newHash)
    {
        if (string.IsNullOrWhiteSpace(newHash))
            throw new DomainException("Password hash cannot be empty.");

        PasswordHash = newHash;
    }

    public void AssignRole(Guid roleId)
    {
        if (_roles.Any(r => r.RoleId == roleId))
            return;

        _roles.Add(new UserRoleAssignment(roleId));
    }

    public void RemoveRole(Guid roleId) => _roles.RemoveAll(r => r.RoleId == roleId);

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

public class UserRoleAssignment
{
    public Guid RoleId { get; private set; }

    private UserRoleAssignment()
    {
    }

    public UserRoleAssignment(Guid roleId)
    {
        RoleId = roleId;
    }
}
