using Microsoft.Extensions.Logging;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.Identity.Application.Abstractions;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.Identity.Domain.Users;
using HrUnitOfWork = SwiftERP.HR.Domain.Shared.IUnitOfWork;
using IdentityUnitOfWork = SwiftERP.Identity.Domain.Shared.IUnitOfWork;

namespace SwiftERP.Identity.Infrastructure.Seeding;

/// <summary>
/// Idempotent startup seeding: guarantees the two system roles named directly in the user's
/// access-rule example ("admin can see everything, an employee just sees inventory") exist, and
/// bootstraps one Admin login so there's always a way into a fresh database. Every check reads
/// current state first and no-ops if it's already seeded, so this is safe to run on every boot.
/// </summary>
public class IdentitySeeder(
    IRoleRepository roleRepository,
    IUserRepository userRepository,
    IEmployeeRepository employeeRepository,
    IPasswordHasher passwordHasher,
    IdentityUnitOfWork identityUnitOfWork,
    HrUnitOfWork hrUnitOfWork,
    ILogger<IdentitySeeder> logger)
{
    public const string DefaultAdminEmail = "admin@swifterp.local";
    public const string DefaultAdminPassword = "Admin@12345";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var adminRole = await EnsureRoleAsync("Admin", isSystemRole: true, cancellationToken,
            (Module.Inventory, AccessLevel.Full),
            (Module.Sales, AccessLevel.Full),
            (Module.Finance, AccessLevel.Full),
            (Module.HR, AccessLevel.Full));

        await EnsureRoleAsync("Employee", isSystemRole: true, cancellationToken,
            (Module.Inventory, AccessLevel.View),
            (Module.Sales, AccessLevel.None),
            (Module.Finance, AccessLevel.None),
            (Module.HR, AccessLevel.None));

        await identityUnitOfWork.SaveChangesAsync(cancellationToken);

        var existingAdmin = await userRepository.GetByEmailAsync(DefaultAdminEmail, cancellationToken);
        if (existingAdmin is not null)
            return;

        var employee = await CreateAdminEmployeeAsync(cancellationToken);

        var admin = new User(employee.Id, DefaultAdminEmail, passwordHasher.Hash(DefaultAdminPassword));
        admin.AssignRole(adminRole.Id);
        userRepository.Add(admin);

        await identityUnitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogWarning(
            "Seeded default admin account {Email} with password {Password} — change this immediately outside local development.",
            DefaultAdminEmail, DefaultAdminPassword);
    }

    private async Task<Role> EnsureRoleAsync(
        string name, bool isSystemRole, CancellationToken cancellationToken,
        params (Module Module, AccessLevel Level)[] permissions)
    {
        var role = await roleRepository.GetByNameAsync(name, cancellationToken);
        if (role is null)
        {
            role = new Role(name, isSystemRole);
            roleRepository.Add(role);
        }

        foreach (var (module, level) in permissions)
            role.SetPermission(module, level);

        return role;
    }

    private async Task<Employee> CreateAdminEmployeeAsync(CancellationToken cancellationToken)
    {
        var employee = new Employee(
            fullName: "System Administrator",
            email: DefaultAdminEmail,
            monthlySalary: 1,
            hireDate: DateOnly.FromDateTime(DateTime.UtcNow),
            jobTitle: "Administrator",
            department: "IT");

        employeeRepository.Add(employee);
        await hrUnitOfWork.SaveChangesAsync(cancellationToken);
        return employee;
    }
}
