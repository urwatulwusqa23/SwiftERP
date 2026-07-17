using SwiftERP.Identity.Domain.Users;

namespace SwiftERP.Identity.Application.Tests.Fakes;

public class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = [];

    public void Seed(User user) => _users.Add(user);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Email == email));

    public Task<User?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken) =>
        Task.FromResult(_users.FirstOrDefault(u => u.EmployeeId == employeeId));

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult(_users.ToList());

    public void Add(User user) => _users.Add(user);
}
