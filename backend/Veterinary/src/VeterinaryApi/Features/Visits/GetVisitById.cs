using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Visits;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Visits;

/// <summary>
/// Vertical slice for retrieving full details of a single visit by identifier,
/// including animal, client, payment, and optional appointment information.
/// </summary>
public static class GetVisitById
{
    /// <summary>Query carrying the target visit identifier.</summary>
    /// <param name="Id">The unique identifier of the visit to retrieve.</param>
    public record Query(Guid Id) : IQuery<Response>;

    /// <summary>
    /// Rich read-model DTO for a visit detail response.
    /// Includes 20 fields covering animal info, client info, appointment (optional),
    /// clinical data, and payment details.
    /// </summary>
    public record Response(
        Guid Id,
        Guid AnimalId,
        string AnimalName,
        string AnimalSpecies,
        string? AnimalBreed,
        Guid ClientId,
        string ClientFullName,
        string ClientPhone,
        Guid? AppointmentId,
        DateTime? AppointmentDate,
        string? AppointmentStatus,
        string VisitType,
        List<string>? Symptoms,
        List<string>? Diagnosis,
        List<string>? Treatment,
        string? Notes,
        DateTime CreatedOnUtc,
        decimal PaymentAmount,
        string PaymentStatus,
        DateTime? UpdatedOnUtc = null);

    /// <summary>Handles the <see cref="Query"/> with tenant-scoped projection including appointment navigation.</summary>
    public class GetVisitByIdQueryHandler : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        /// <summary>Initializes the handler with database and tenant context services.</summary>
        public GetVisitByIdQueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        /// <summary>
        /// Projects the visit (with <c>Include(e =&gt; e.Appointment)</c>) to a <see cref="Response"/> DTO.
        /// </summary>
        /// <returns>A successful result, or <c>VisitErrors.VisitNotFound</c>.</returns>
        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var visit = await _db.Visits
                .ForTenant(_currentTenant.UserId!.Value)
                .AsNoTracking()
                .Where(e => e.Id == query.Id)
                .Include(e => e.Appointment)
                .Select(e => new Response(
                    e.Id,
                    e.Animal.Id,
                    e.Animal.Name,
                    e.Animal.Species,
                    e.Animal.Breed,
                    e.Owner.Id,
                    e.Owner.FullName,
                    e.Owner.Phone,
                    e.AppointmentId == null ? null : e.AppointmentId,
                    e.AppointmentId == null ? null : e.Appointment!.AppointmentDate,
                    e.AppointmentId == null ? null : e.Appointment!.Status.ToString(),
                    e.VisitType.ToString(),
                    e.Symptoms,
                    e.Diagnosis,
                    e.Treatment,
                    e.Notes,
                    e.CreatedOnUtc,
                    e.PaymentAmount,
                    e.PaymentStatus.ToString()))
                .FirstOrDefaultAsync(cancellationToken);

            if (visit is null)
            {
                return Result<Response>.Failure(VisitErrors.VisitNotFound(query.Id));
            }
            return Result<Response>.Success(visit);
        }
    }

    /// <summary>Carter endpoint that maps <c>GET /visits/{id}</c>. Requires authorization.</summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the get-visit-by-id route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/visits/{id:guid}", [Authorize] async (
                Guid id,
                IQueryHandler<Query, Response> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new Query(id);
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();
            })
            .WithTags($"{nameof(Visit)}s")
            .WithSummary("Get visit by ID")
            .WithDescription("Retrieves detailed information about a specific visit including animal details, client information, appointment data, symptoms, diagnosis, and treatment.");
        }
    }
}
