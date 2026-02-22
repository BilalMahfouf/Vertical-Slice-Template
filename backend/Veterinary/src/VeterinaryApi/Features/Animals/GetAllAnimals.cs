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
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Animals;

/// <summary>
/// Vertical slice for retrieving a paginated, filterable, and sortable list of animals
/// belonging to the currently authenticated tenant.
/// </summary>
public static class GetAllAnimals
{
    /// <summary>Read-model DTO representing a single animal row in the results table.</summary>
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

    /// <summary>
    /// Handles <see cref="TableRequest{T}"/> for the animals list.
    /// Applies tenant scoping, optional full-text search, and offset pagination.
    /// ⚠️ Warning: sorting is applied in-memory after <c>ToListAsync</c>.
    /// </summary>
    public class GetAllAnimalsQueryHandler
        : IQueryHandler<TableRequest<Response>, OffSetPagedList<Response>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        /// <summary>Initializes the handler with database and tenant context services.</summary>
        public GetAllAnimalsQueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        /// <summary>
        /// Projects tenant-scoped animals to <see cref="Response"/> DTOs, applies search and sort,
        /// and paginates.
        /// </summary>
        public async Task<Result<OffSetPagedList<Response>>> Handle(
            TableRequest<Response> query,
            CancellationToken cancellationToken = default)
        {
            var count = await _db.Animals
                .ForTenant(_currentTenant.UserId!.Value)
                .CountAsync(cancellationToken);
            if (count <= 0)
            {
                return Result<OffSetPagedList<Response>>.Failure(
                    AnimalErrors.AnimalsNotFound);
            }
            var animal = _db.Animals
                .ForTenant(_currentTenant.UserId!.Value)
                .AsNoTracking();
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
                _ => e => e.CreatedOnUtc
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
    /// <summary>
    /// Carter endpoint that maps <c>GET /animals</c>.
    /// Requires authorization. Accepts optional query params for pagination, search, and sorting.
    /// </summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the get-all-animals route.</summary>
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