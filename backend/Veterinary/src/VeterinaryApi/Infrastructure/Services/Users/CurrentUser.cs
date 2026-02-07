using System.Security.Claims;
using VeterinaryApi.Common.Abstracions;

namespace VeterinaryApi.Infrastructure.Services.Users;


internal class CurrentUserService : ICurrentTenant
{
    private readonly IHttpContextAccessor _contextAccessor;

    public CurrentUserService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userId = _contextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userId, out Guid id) ? id : null;
        }
    }
}
