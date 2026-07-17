using SwiftERP.Identity.Application.Abstractions;
using SwiftERP.Identity.Domain.Roles;

namespace SwiftERP.Identity.Application.Tests.Fakes;

public class FakeTokenIssuer : ITokenIssuer
{
    public IReadOnlyDictionary<Module, AccessLevel>? LastPermissions { get; private set; }
    public bool? LastIsSystemAdmin { get; private set; }

    public IssuedToken Issue(
        Guid userId,
        Guid employeeId,
        string email,
        IReadOnlyDictionary<Module, AccessLevel> permissions,
        bool isSystemAdmin)
    {
        LastPermissions = permissions;
        LastIsSystemAdmin = isSystemAdmin;
        return new IssuedToken("fake-token", DateTimeOffset.UtcNow.AddHours(2));
    }
}
