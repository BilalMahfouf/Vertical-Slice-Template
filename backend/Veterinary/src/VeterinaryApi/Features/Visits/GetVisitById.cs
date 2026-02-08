using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Features.Visits;

public static class GetVisitById
{
    public record Query(Guid Id) : IQuery<Response>;

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
        DateTime? UpdatedOnUtc = null);

    public class GetVisitByIdQueryHandler : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _db;

        public GetVisitByIdQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var visit = await _db.Visits
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
                    e.CreatedOnUtc))
                .FirstOrDefaultAsync(cancellationToken);

            if (visit is null)
            {
                return Result<Response>.Failure(VisitErrors.VisitNotFound(query.Id));
            }
            return Result<Response>.Success(visit);
        }
    }

    public class Endpoint : IEndpoint
    {
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
