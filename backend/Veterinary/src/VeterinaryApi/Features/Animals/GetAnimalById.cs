using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain;
using VeterinaryApi.Domain.Animals;

namespace VeterinaryApi.Features.Animals;

public static class GetAnimalById
{
    public record Query(Guid Id) : IQuery<Response>;
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

    public class GetAnimalByIdQueryHandler : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _db;
        public GetAnimalByIdQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var animal = await _db.Animals
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
    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/animals/{id:guid}",[Authorize] async (
                Guid id,
                IQueryHandler<Query, Response> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new Query(id);
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();
            }).WithTags("animals");
        }
    }
}
