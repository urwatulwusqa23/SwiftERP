using SwiftERP.Identity.Infrastructure.Security;

namespace SwiftERP.Identity.Infrastructure.Tests;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Verify_succeeds_for_the_correct_password()
    {
        var hash = _hasher.Hash("Admin@12345");

        Assert.True(_hasher.Verify("Admin@12345", hash));
    }

    [Fact]
    public void Verify_fails_for_the_wrong_password()
    {
        var hash = _hasher.Hash("Admin@12345");

        Assert.False(_hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_is_salted_so_the_same_password_hashes_differently_each_time()
    {
        var hash1 = _hasher.Hash("Admin@12345");
        var hash2 = _hasher.Hash("Admin@12345");

        Assert.NotEqual(hash1, hash2);
        Assert.True(_hasher.Verify("Admin@12345", hash1));
        Assert.True(_hasher.Verify("Admin@12345", hash2));
    }

    [Fact]
    public void Verify_returns_false_rather_than_throwing_for_a_malformed_hash()
    {
        Assert.False(_hasher.Verify("Admin@12345", "not-a-valid-hash"));
    }
}
