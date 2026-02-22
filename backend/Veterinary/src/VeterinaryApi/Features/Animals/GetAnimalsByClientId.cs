using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations.OffSet;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Animals;

/// <summary>
/// Vertical slice for retrieving a list of animals belonging to a specific client,
/// scoped to the currently authenticated tenant.
/// </summary>
public static class GetAnimalsByClientId
{
    /// <summary>
    /// Query carrying the client identifier to filter by.
    /// Implements <see cref="IQuery{TResponse}"/> where the response is a paged list.
    /// </summary>
    /// <param name="ClientId">The unique identifier of the client whose animals to retrieve.</param>
    public sealed record Query(Guid ClientId) : IQuery<OffSetPagedList<Response>>;

    /// <summary>Lightweight DTO for listing an animal as part of a client's record.</summary>
    /// <param name="AnimalId">The animal's unique identifier.</param>
    /// <param name="Name">The animal's name.</param>
    /// <param name="Species">The animal's species.</param>
    public sealed record Response(Guid AnimalId, string Name, string Species);

    /// <summary>
    /// Handles the <see cref="Query"/> by loading all tenant-scoped animals for the given client.
    /// Returns all animals in a single page (no server-side pagination applied).
    /// </summary>
    public sealed class GetAnimalsByClientIdQueryHandler
        : IQueryHandler<Query, OffSetPagedList<Response>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        /// <summary>Initializes the handler with database and tenant context services.</summary>
        public GetAnimalsByClientIdQueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        /// <summary>
        /// Projects animals to <see cref="Response"/> DTOs filtered by <paramref name="query"/>.ClientId.
        /// </summary>
        /// <returns>A paged list with all results in one page, or <c>AnimalErrors.AnimalsNotFound</c>.</returns>
        public async Task<Result<OffSetPagedList<Response>>> Handle(
            Query query,
            CancellationToken cancellationToken = default)
        {
            var animals = await _db.Animals
                .ForTenant(_currentTenant.UserId!.Value)
                .AsNoTracking()
                .Where(e => e.ClientId == query.ClientId)
                .Select(e => new Response(
                    e.Id,
                    e.Name,
                    e.Species))
                .ToListAsync(cancellationToken);
            if (animals is null || !animals.Any())
            {
                return Result<OffSetPagedList<Response>>.Failure(
                    AnimalErrors.AnimalsNotFound);
            }
            var result = OffSetPagedList<Response>.Create(
                animals,
                animals.Count,
                1,
                animals.Count);
            return Result<OffSetPagedList<Response>>.Success(result);
        }
    }
    /// <summary>Carter endpoint that maps <c>GET /animals/by-client/{clientId}</c>. Requires authorization.</summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the get-animals-by-client route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/animals/by-client/{clientId:guid}", [Authorize] async (
                Guid clientId,
                IQueryHandler<Query, OffSetPagedList<Response>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new Query(clientId);
                var result = await handler
                    .Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();
            })
            .WithTags($"{nameof(Animal)}s")
            .WithSummary("Get animals by client ID")
            .WithDescription("Retrieves all animals belonging to a specific client by client ID.");
        }
    }
}
