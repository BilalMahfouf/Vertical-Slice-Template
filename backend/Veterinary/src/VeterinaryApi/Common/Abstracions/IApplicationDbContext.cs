using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Common.Abstracions;

public interface IApplicationDbContext
{
    public DbSet<User> Users { get; }
    public DbSet<UserSession> UserSessions { get; }


    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
