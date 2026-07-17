using SwiftERP.Identity.Domain.Roles;

namespace SwiftERP.Identity.Application.Tests.Fakes;

public class FakeRoleRepository : IRoleRepository
{
    private readonly List<Role> _roles = [];

    public void Seed(Role role) => _roles.Add(role);

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_roles.FirstOrDefault(r => r.Id == id));

    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken) =>
        Task.FromResult(_roles.FirstOrDefault(r => r.Name == name));

    public Task<List<Role>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult(_roles.ToList());

    public void Add(Role role) => _roles.Add(role);
}
