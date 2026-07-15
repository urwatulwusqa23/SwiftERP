using Microsoft.EntityFrameworkCore;
using SwiftERP.Identity.Domain.Users;

namespace SwiftERP.Identity.Infrastructure.Persistence.Repositories;

public class UserRepository(IdentityDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken) =>
        dbContext.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.EmployeeId == employeeId, cancellationToken);

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.Users.Include(u => u.Roles).ToListAsync(cancellationToken);

    public void Add(User user) => dbContext.Users.Add(user);
}
