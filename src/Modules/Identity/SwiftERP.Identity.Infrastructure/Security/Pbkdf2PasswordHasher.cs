using System.Security.Cryptography;
using SwiftERP.Identity.Application.Abstractions;

namespace SwiftERP.Identity.Infrastructure.Security;

/// <summary>
/// PBKDF2-HMAC-SHA256 password hashing using .NET's built-in Rfc2898DeriveBytes rather than
/// pulling in the Microsoft.AspNetCore.Identity package — that package's PasswordHasher would
/// have worked too, but its dependency chain drags in System.Security.Cryptography.Xml, which
/// has an open high-severity advisory (unrelated to the XML-free code path we'd actually use).
/// Format: {iterations}.{salt-base64}.{hash-base64} — self-describing so the iteration count
/// can be bumped later without invalidating already-stored hashes.
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            return false;

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
