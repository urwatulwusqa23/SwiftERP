using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SwiftERP.Identity.Application.Abstractions;
using SwiftERP.Identity.Domain.Roles;

namespace SwiftERP.Identity.Infrastructure.Security;

public class JwtTokenIssuer(IOptions<JwtOptions> options) : ITokenIssuer
{
    public IssuedToken Issue(
        Guid userId,
        Guid employeeId,
        string email,
        IReadOnlyDictionary<Module, AccessLevel> permissions,
        bool isSystemAdmin)
    {
        var jwtOptions = options.Value;
        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(SwiftErpClaimTypes.EmployeeId, employeeId.ToString()),
            new(SwiftErpClaimTypes.IsSystemAdmin, isSystemAdmin ? "true" : "false"),
        };

        // Baking effective permissions into the token itself (rather than looking them up from
        // the DB on every request) trades "changes take effect on next login" for zero DB
        // round-trips during authorization — the right call for this project's scale, and
        // documented as a known limitation in the README.
        claims.AddRange(permissions.Select(p =>
            new Claim(SwiftErpClaimTypes.ModuleClaim(p.Key.ToString()), p.Value.ToString())));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new IssuedToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
