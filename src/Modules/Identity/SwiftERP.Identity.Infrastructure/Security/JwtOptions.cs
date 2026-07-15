namespace SwiftERP.Identity.Infrastructure.Security;

public class JwtOptions
{
    public string SigningKey { get; set; } = default!;
    public string Issuer { get; set; } = "SwiftERP";
    public string Audience { get; set; } = "SwiftERP";
    public int ExpiryMinutes { get; set; } = 120;
}
