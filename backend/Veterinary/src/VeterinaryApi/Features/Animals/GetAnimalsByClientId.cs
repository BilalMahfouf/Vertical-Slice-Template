using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations.OffSet;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Animals;

namespace VeterinaryApi.Features.Animals;

public static class GetAnimalsByClientId
{
    public sealed record Query(Guid ClientId) : IQuery<OffSetPagedList<Response>>;
    public sealed record Response(Guid AnimalId, string Name, string Species);

    public sealed class GetAnimalsByClientIdQueryHandler
        : IQueryHandler<Query, OffSetPagedList<Response>>
    {
        private readonly IApplicationDbContext _db;
        public GetAnimalsByClientIdQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Result<OffSetPagedList<Response>>> Handle(
            Query query,
            CancellationToken cancellationToken = default)
        {
            var animals = await _db.Animals
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
    public sealed class Endpoint : IEndpoint
    {
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
