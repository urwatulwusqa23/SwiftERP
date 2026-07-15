using SwiftERP.Identity.Domain.Roles;

namespace SwiftERP.Identity.Application.Abstractions;

public record IssuedToken(string Token, DateTimeOffset ExpiresAtUtc);

public interface ITokenIssuer
{
    IssuedToken Issue(
        Guid userId,
        Guid employeeId,
        string email,
        IReadOnlyDictionary<Module, AccessLevel> permissions,
        bool isSystemAdmin);
}
