using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Clients;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Domain.Users;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Common.Abstracions;

public interface IApplicationDbContext
{
    public DbSet<User> Users { get; }
    public DbSet<UserSession> UserSessions { get; }
    public DbSet<Clinic> Clinics { get; }
    public DbSet<Client> Clients { get; }
    public DbSet<Animal> Animals { get; }
    public DbSet<Appointment> Appointments { get; }
    public DbSet<Visit> Visits { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
