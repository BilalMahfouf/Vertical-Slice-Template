using Isopoh.Cryptography.Argon2;
using VeterinaryApi.Common.Abstracions;

namespace VeterinaryApi.Infrastructure.Services.Hashers;

public class Argon2PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return Argon2.Hash(password);
    }

    public bool Verify(string password, string hash)
    {
        return Argon2.Verify(hash, password);
    }
}
