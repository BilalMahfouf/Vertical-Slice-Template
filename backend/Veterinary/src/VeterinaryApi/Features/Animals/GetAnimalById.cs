using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain;
using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Animals;

/// <summary>
/// Vertical slice for retrieving a single animal by its unique identifier,
/// scoped to the currently authenticated tenant.
/// </summary>
public static class GetAnimalById
{
    /// <summary>
    /// Query carrying the target animal's identifier.
    /// Implements <see cref="IQuery{TResponse}"/> where the response is <see cref="Response"/>.
    /// </summary>
    /// <param name="Id">The unique identifier of the animal to retrieve.</param>
    public record Query(Guid Id) : IQuery<Response>;

    /// <summary>Detailed read-model DTO for a single animal lookup.</summary>
    public record Response(
        Guid Id,
        Guid ClinicId,
        Guid ClientId,
        string ClientName,
        string Name,
        string Species,
        string? Breed,
        Gender Gender,
        DateTime? BirthDate,
        string? Color,
        string? MicrochipNumber,
        string status);

    /// <summary>Handles the <see cref="Query"/> with a tenant-scoped, non-tracked projection.</summary>
    public class GetAnimalByIdQueryHandler : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        /// <summary>Initializes the handler with database and tenant context services.</summary>
        public GetAnimalByIdQueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        /// <summary>Projects the animal and its client to a <see cref="Response"/> DTO.</summary>
        /// <returns>A successful result, or <c>AnimalErrors.AnimalNotFound</c>.</returns>
        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var animal = await _db.Animals
                .ForTenant(_currentTenant.UserId!.Value)
                .AsNoTracking()
                .Where(e => e.Id == query.Id)
                .Select(e => new Response(
                    e.Id,
                    e.ClinicId,
                    e.ClientId,
                    e.Client.FullName,
                    e.Name,
                    e.Species,
                    e.Breed,
                    e.Gender,
                    e.BirthDate,
                    e.Color,
                    e.MicrochipNumber,
                    e.Status.ToString()))
                .FirstOrDefaultAsync(cancellationToken);

            if (animal is null)
            {
                return Result<Response>
                    .Failure(AnimalErrors.AnimalNotFound(query.Id));
            }
            return Result<Response>.Success(animal);
        }
    }
    /// <summary>Carter endpoint that maps <c>GET /animals/{id}</c>. Requires authorization.</summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the get-animal-by-id route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/animals/{id:guid}", [Authorize] async (
                Guid id,
                IQueryHandler<Query, Response> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new Query(id);
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();
            })
            .WithTags($"{nameof(Animal)}s")
            .WithSummary("Get animal by ID")
            .WithDescription("Retrieves detailed information about a specific animal by its unique identifier.");
        }
    }
}
