using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations.OffSet;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain;
using VeterinaryApi.Domain.Animals;

namespace VeterinaryApi.Features.Animals;

public static class GetAllAnimals
{

    public record Response(
        Guid Id,
        Guid ClinicId,
        Guid ClientId,
        string ClientName,
        string ClientPhone,
        string Name,
        string Species,
        string? Breed,
        string Gender,
        DateTime? BirthDate,
        string? Color,
        string? MicrochipNumber,
        DateTime CreatedOnUtc,
        string status);

    public class GetAllAnimalsQueryHandler
        : IQueryHandler<TableRequest<Response>, OffSetPagedList<Response>>
    {
        private readonly IApplicationDbContext _db;

        public GetAllAnimalsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result<OffSetPagedList<Response>>> Handle(
            TableRequest<Response> query,
            CancellationToken cancellationToken = default)
        {
            var count = await _db.Animals.CountAsync(cancellationToken);
            if (count <= 0)
            {
                return Result<OffSetPagedList<Response>>.Failure(
                    AnimalErrors.AnimalsNotFound);
            }
            var animal = _db.Animals.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(query.search))
            {
                animal = animal.Where(e =>
                    e.Name.ToLower().Contains(query.search) ||
                    e.Species.ToLower().Contains(query.search) ||
                    (e.Breed != null && e.Breed.ToLower().Contains(query.search)) ||
                    e.Client.FullName.ToLower().Contains(query.search));
            }

            var animals = animal.Select(e => new Response(
                                            e.Id,
                                            e.ClinicId,
                                            e.ClientId,
                                            e.Client.FullName,
                                            e.Clinic.Phone,
                                            e.Name,
                                            e.Species,
                                            e.Breed,
                                            e.Gender.ToString(),
                                            e.BirthDate,
                                            e.Color,
                                            e.MicrochipNumber,
                                            e.CreatedOnUtc,
                                            e.Status.ToString()));


            Expression<Func<Response, object>> orderSelector = query.SortColumn?
                .ToLower() switch
            {
                "name" => e => e.Name,
                "species" => e => e.Species,
                "breed" => e => e.Breed ?? string.Empty,
                "clientname" => e => e.ClientName,
                _ => e => e.Id
            };
            var temp = await animals.ToListAsync(cancellationToken);
            if (temp is null)
            {
                return Result<OffSetPagedList<Response>>
                    .Failure(AnimalErrors.AnimalsNotFound);
            }

            var animalsQuery = temp.AsQueryable();
            if (query.SortOrder is "desc")
            {
                animalsQuery = animalsQuery.OrderByDescending(orderSelector);
            }
            else
            {
                animalsQuery = animalsQuery.OrderBy(orderSelector);
            }

            animalsQuery = animalsQuery.Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var data = animalsQuery.ToList();
            if (data is null)
            {
                return Result<OffSetPagedList<Response>>
                    .Failure(AnimalErrors.AnimalsNotFound);
            }
            var result = OffSetPagedList<Response>
                .Create(data, count, query.Page, query.PageSize);
            return Result<OffSetPagedList<Response>>.Success(result);
        }
    }
    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/animals", [Authorize] async (
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                [FromQuery] string? sortColumn,
                [FromQuery] string? sortOrder,
                [FromQuery] string? search,
                [FromServices] IQueryHandler<TableRequest<Response>, OffSetPagedList<Response>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = TableRequest<Response>
                    .Create(pageSize, page, search, sortColumn, sortOrder);
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();

            })
            .WithTags($"{nameof(Animal)}s")
            .WithSummary("Get all animals")
            .WithDescription("Retrieves a paginated list of all animals with optional search, sorting, and filtering capabilities.");
        }
    }
}