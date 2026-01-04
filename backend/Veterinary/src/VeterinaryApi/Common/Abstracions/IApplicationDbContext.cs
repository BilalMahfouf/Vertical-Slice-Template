using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Common.Abstracions;

public interface IApplicationDbContext
{
    public DbSet<User> Users { get; }
    public DbSet<UserSession> UserSessions { get; }
    public DbSet<Clinic> Clinics { get; }


    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
