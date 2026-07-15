using SwiftERP.SharedKernel;

namespace SwiftERP.Identity.Domain.Roles;

public class Role : Entity
{
    public string Name { get; private set; } = default!;
    public bool IsSystemRole { get; private set; }

    private readonly List<ModulePermission> _permissions = [];
    public IReadOnlyCollection<ModulePermission> Permissions => _permissions.AsReadOnly();

    private Role()
    {
    }

    public Role(string name, bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name cannot be empty.");

        Name = name;
        IsSystemRole = isSystemRole;

        // Every role starts with an explicit "None" row per module rather than an implicit
        // default, so the permission matrix always has something to show/edit for every role.
        foreach (var module in Enum.GetValues<Module>())
            _permissions.Add(new ModulePermission(module, AccessLevel.None));
    }

    /// Permission levels stay editable even for system roles (an admin may reasonably want to
    /// tune what the seeded "Employee" role can see) — IsSystemRole only protects the role's
    /// name and existence, not its permission matrix.
    public void SetPermission(Module module, AccessLevel level)
    {
        var existing = _permissions.FirstOrDefault(p => p.Module == module);
        if (existing is null)
        {
            _permissions.Add(new ModulePermission(module, level));
            return;
        }

        existing.SetLevel(level);
    }

    public AccessLevel GetPermission(Module module) =>
        _permissions.FirstOrDefault(p => p.Module == module)?.AccessLevel ?? AccessLevel.None;

    public void Rename(string name)
    {
        if (IsSystemRole)
            throw new DomainException($"Role '{Name}' is a system role and cannot be renamed.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name cannot be empty.");

        Name = name;
    }
}

public class ModulePermission
{
    public Module Module { get; private set; }
    public AccessLevel AccessLevel { get; private set; }

    private ModulePermission()
    {
    }

    public ModulePermission(Module module, AccessLevel accessLevel)
    {
        Module = module;
        AccessLevel = accessLevel;
    }

    public void SetLevel(AccessLevel level) => AccessLevel = level;
}
