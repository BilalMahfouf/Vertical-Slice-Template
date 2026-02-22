using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Appointments;

/// <summary>
/// Vertical slice for retrieving a single appointment by its unique identifier,
/// scoped to the currently authenticated tenant.
/// </summary>
public static class GetAppointmentById
{
    /// <summary>Query carrying the target appointment ID.</summary>
    public record Query(Guid Id) : IQuery<Response>;

    /// <summary>Appointment detail DTO returned for a single-record lookup.</summary>
    /// <param name="Id">The appointment's unique identifier.</param>
    /// <param name="ClinicId">The clinic at which the appointment is scheduled.</param>
    /// <param name="ClientId">The animal owner's unique identifier.</param>
    /// <param name="ClientName">Full name of the animal owner.</param>
    /// <param name="AnimalName">Name of the animal being seen.</param>
    /// <param name="AppointmentDate">The scheduled date and time.</param>
    /// <param name="Status">The stringified appointment status.</param>
    /// <param name="CreatedOnUtc">UTC creation timestamp.</param>
    public record Response(
        Guid Id,
        Guid ClinicId,
        Guid ClientId,
        string ClientName,
        string AnimalName,
        DateTime AppointmentDate,
        string Status,
        DateTime CreatedOnUtc);

    /// <summary>
    /// Resolves a single appointment from the database, filtered by the tenant and the query ID.
    /// </summary>
    public class GetAppointmentByIdQueryHandler : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        /// <summary>Initializes the handler with database context and tenant accessor.</summary>
        public GetAppointmentByIdQueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        /// <summary>
        /// Queries the database for the specified appointment within the current tenant's scope.
        /// </summary>
        /// <param name="query">The query containing the appointment ID to look up.</param>
        /// <param name="cancellationToken">Token for cooperative cancellation.</param>
        /// <returns>
        /// A successful <see cref="Result{T}"/> with the appointment detail DTO,
        /// or a failure with <c>AppointmentErrors.AppointmentNotFound</c>.
        /// </returns>
        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var appointment = await _db.Appointments
                .ForTenant(_currentTenant.UserId!.Value)
                .AsNoTracking()
                .Where(e => e.Id == query.Id)
                .Select(e => new Response(
                    e.Id,
                    e.ClinicId,
                    e.Animal.ClientId,
                    e.Animal.Client.FullName,
                    e.Animal.Name,
                    e.AppointmentDate,
                    e.Status.ToString(),
                    e.CreatedOnUtc))
                .FirstOrDefaultAsync(cancellationToken);

            if (appointment is null)
            {
                return Result<Response>
                    .Failure(AppointmentErrors.AppointmentNotFound(query.Id));
            }
            return Result<Response>.Success(appointment);
        }
    }

    /// <summary>
    /// Carter endpoint that maps <c>GET /appointments/{id}</c>.
    /// Returns <c>200 OK</c> with the appointment detail or a Problem Details error.
    /// </summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the get-by-id route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/appointments/{id:guid}", [Authorize] async (
                Guid id,
                IQueryHandler<Query, Response> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new Query(id);
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();
            })
            .WithTags($"{nameof(Appointment)}s")
            .WithSummary("Get appointment by ID")
            .WithDescription("Retrieves detailed information about a specific appointment by its unique identifier.");
        }
    }
}
