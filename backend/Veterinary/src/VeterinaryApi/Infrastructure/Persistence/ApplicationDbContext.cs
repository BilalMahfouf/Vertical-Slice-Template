using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Clients;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Domain.Notifications;
using VeterinaryApi.Domain.Users;
using VeterinaryApi.Domain.Vaccinations;
using VeterinaryApi.Domain.Visits;
using VeterinaryApi.Infrastructure.OutboxMessages;
using VeterinaryApi.Infrastructure.Persistence.Configurations.Users;

namespace VeterinaryApi.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
    , IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserSession> UserSessions { get; set; } = null!;
    public DbSet<Clinic> Clinics { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Animal> Animals { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<Visit> Visits { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<Vaccination> Vaccinations { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
    }

}
