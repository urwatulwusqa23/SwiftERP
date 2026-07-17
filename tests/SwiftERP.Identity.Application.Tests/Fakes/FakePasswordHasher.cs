using SwiftERP.Identity.Application.Abstractions;

namespace SwiftERP.Identity.Application.Tests.Fakes;

// Deliberately trivial (no real hashing) — these tests exercise the login handler's business
// logic, not password security, which is already covered by Pbkdf2PasswordHasherTests.
public class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hashed:{password}";

    public bool Verify(string password, string hash) => hash == $"hashed:{password}";
}
