using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Clients;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Domain.Notifications;
using VeterinaryApi.Domain.Users;
using VeterinaryApi.Domain.Vaccinations;
using VeterinaryApi.Domain.Visits;
using VeterinaryApi.Infrastructure.OutboxMessages;

namespace VeterinaryApi.Common.Abstracions;

/// <summary>
/// Defines the abstraction layer over the EF Core <c>ApplicationDbContext</c>.
/// Feature handlers depend on this interface rather than the concrete DbContext,
/// enabling testability (mock or in-memory implementations) and decoupling
/// the application logic from the infrastructure persistence layer.
/// </summary>
/// <remarks>
/// Registered in DI with scoped lifetime. Handlers receive this interface via constructor injection.
/// All write operations must call <see cref="SaveChangesAsync"/> to persist changes.
/// The concrete <c>ApplicationDbContext</c> implements this interface and applies
/// EF Core interceptors for tenant stamping, audit tracking, and outbox message insertion.
/// </remarks>
public interface IApplicationDbContext
{
    /// <summary>Gets the EF Core <see cref="DbSet{T}"/> for <see cref="User"/> entities.</summary>
    public DbSet<User> Users { get; }

    /// <summary>Gets the EF Core <see cref="DbSet{T}"/> for <see cref="UserSession"/> entities (refresh tokens).</summary>
    public DbSet<UserSession> UserSessions { get; }

    /// <summary>Gets the EF Core <see cref="DbSet{T}"/> for <see cref="Clinic"/> entities.</summary>
    public DbSet<Clinic> Clinics { get; }

    /// <summary>Gets the EF Core <see cref="DbSet{T}"/> for <see cref="Client"/> entities (pet owners).</summary>
    public DbSet<Client> Clients { get; }

    /// <summary>Gets the EF Core <see cref="DbSet{T}"/> for <see cref="Animal"/> entities (patients).</summary>
    public DbSet<Animal> Animals { get; }

    /// <summary>Gets the EF Core <see cref="DbSet{T}"/> for <see cref="Appointment"/> entities.</summary>
    public DbSet<Appointment> Appointments { get; }

    /// <summary>Gets the EF Core <see cref="DbSet{T}"/> for <see cref="Visit"/> entities (clinical records).</summary>
    public DbSet<Visit> Visits { get; }

    /// <summary>
    /// Gets the EF Core <see cref="DbSet{T}"/> for <see cref="OutboxMessage"/> entities.
    /// Used by the <c>InsertOutboxMessagesInterceptors</c> and <c>ProcessOutboxMessagesJob</c>.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages { get; }

    /// <summary>Gets the EF Core <see cref="DbSet{T}"/> for <see cref="Notification"/> entities.</summary>
    public DbSet<Notification> Notifications { get; }

    /// <summary>Gets the EF Core <see cref="DbSet{T}"/> for <see cref="Vaccination"/> entities.</summary>
    public DbSet<Vaccination> Vaccinations { get; }

    /// <summary>
    /// Asynchronously saves all pending changes in this unit of work to the database.
    /// This also triggers the EF Core interceptors (tenant stamping, outbox insertion, audit).
    /// </summary>
    /// <param name="cancellationToken">Token to observe for async cancellation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
