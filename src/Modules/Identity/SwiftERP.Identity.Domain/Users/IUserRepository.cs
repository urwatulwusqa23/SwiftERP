namespace SwiftERP.Identity.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken);
    void Add(User user);
}
