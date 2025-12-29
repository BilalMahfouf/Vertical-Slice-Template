using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Common.Abstracions;

public interface IJwtProvider
{
    public string GenerateToken(User user);
    public string GenerateRefreshToken();
    public DateTimeOffset GetTokenExpiration();
}
