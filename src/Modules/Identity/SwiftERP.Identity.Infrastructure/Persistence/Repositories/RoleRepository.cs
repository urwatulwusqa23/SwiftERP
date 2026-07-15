using Microsoft.EntityFrameworkCore;
using SwiftERP.Identity.Domain.Roles;

namespace SwiftERP.Identity.Infrastructure.Persistence.Repositories;

public class RoleRepository(IdentityDbContext dbContext) : IRoleRepository
{
    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken) =>
        dbContext.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

    public Task<List<Role>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.Roles.Include(r => r.Permissions).ToListAsync(cancellationToken);

    public void Add(Role role) => dbContext.Roles.Add(role);
}
