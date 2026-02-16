using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Vaccinations;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Vaccinations;

/// <summary>
/// Feature for retrieving vaccination records by ID.
/// </summary>
public static class GetVaccinationById
{
    /// <summary>
    /// Query to retrieve a vaccination record by its unique identifier.
    /// </summary>
    /// <param name="Id">The unique identifier of the vaccination to retrieve.</param>
    public sealed record Query(Guid Id) : IQuery<Response>;
    /// <summary>
    /// Response containing detailed vaccination information.
    /// </summary>
    /// <param name="Id">The unique identifier of the vaccination.</param>
    /// <param name="AnimalId">The unique identifier of the vaccinated animal.</param>
    /// <param name="AnimalName">The name of the vaccinated animal.</param>
    /// <param name="ClientId">The unique identifier of the client (animal owner).</param>
    /// <param name="ClientName">The full name of the client (animal owner).</param>
    /// <param name="VisitId">The unique identifier of the associated visit, if any.</param>
    /// <param name="VaccinationName">The name of the vaccination administered.</param>
    /// <param name="GivenAt">The date and time when the vaccination was administered.</param>
    /// <param name="DueTo">The date and time when the next vaccination is due.</param>
    /// <param name="Notes">Optional notes or remarks about the vaccination.</param>
    public sealed record Response(
        Guid Id,
        Guid AnimalId,
        string AnimalName,
        Guid ClientId,
        string ClientName,
        Guid? VisitId,
        string VaccinationName,
        DateTime GivenAt,
        DateTime? DueTo,
        string? Notes);
    /// <summary>
    /// Handler for retrieving vaccination records by ID.
    /// Performs a join with Animal and Client tables to gather complete vaccination information.
    /// </summary>
    public sealed class GetVaccinationByIdQueryHandler
        : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        public GetVaccinationByIdQueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }
        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken = default)
        {
            var vaccination = await _db.Vaccinations
                .ForTenant(_currentTenant.UserId!.Value)
                .Where(e => e.Id == query.Id)
                .Select(e => new Response(
                    e.Id,
                    e.AnimalId,
                    e.Animal.Name,
                    e.Animal.ClientId,
                    e.Animal.Client.FullName,
                    e.VisitId,
                    e.Name,
                    e.GivenAt,
                    e.DueTo,
                    e.Notes))
                .FirstOrDefaultAsync(cancellationToken);
            if (vaccination is null)
            {
                return Result<Response>
                    .Failure(VaccinationErrors.NotFound);
            }
            return Result<Response>.Success(vaccination);
        }
    }
    /// <summary>
    /// Endpoint configuration for the get vaccination by ID route.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>
        /// Registers the GET /vaccinations/{id} endpoint.
        /// </summary>
        /// <remarks>
        /// Retrieves a vaccination record by its ID with related animal and client information.
        /// Requires authorization.
        /// Returns 200 OK with the vaccination details or a problem response on failure.
        /// </remarks>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/vaccinations/{id:guid}", async (
                Guid id,
                IQueryHandler<Query, Response> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new Query(id);
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value)
                : result.Problem();

            })
            .WithTags($"{nameof(Vaccination)}s")
            .WithSummary("Get vaccination by ID")
            .WithDescription("Retrieves a vaccination record by its ID with related animal and client information.")
            .RequireAuthorization();
        }
    }
}

