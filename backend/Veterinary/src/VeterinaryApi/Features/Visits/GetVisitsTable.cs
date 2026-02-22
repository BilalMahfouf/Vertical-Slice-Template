using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations.OffSet;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Visits;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Visits;

/// <summary>
/// Vertical slice for retrieving a paginated, filterable, and sortable table of visits.
/// Note: this class is <c>public class</c> (not <c>static</c>), unlike other slice containers.
/// </summary>
public class GetVisitsTable
{
    /// <summary>Row DTO for the visits table view.</summary>
    public record Response(
        Guid Id,
        string VisitType,
        Guid AnimalId,
        string AnimalName,
        string AnimalSpecies,
        Guid OwnerId,
        string OwnerName,
        DateTime VisitDate,
        decimal PaymentAmount,
        string PaymentStatus);

    /// <summary>
    /// Handles <see cref="TableRequest{T}"/> for the visits table.
    /// ⚠️ Warning: sorting is applied in-memory (<c>ToListAsync</c> then <c>AsQueryable</c>).
    /// Supports search by animal name or owner name.
    /// </summary>
    public class GetVisitsTableQueryHandler
        : IQueryHandler<TableRequest<Response>, OffSetPagedList<Response>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        /// <summary>Initializes the handler with database and tenant context services.</summary>
        public GetVisitsTableQueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        /// <summary>
        /// Projects visits to <see cref="Response"/> DTOs, applies optional search/sort,
        /// and returns an offset-paginated result.
        /// </summary>
        public async Task<Result<OffSetPagedList<Response>>> Handle(
            TableRequest<Response> query,
            CancellationToken cancellationToken = default)
        {
            var count = await _db.Visits
                .ForTenant(_currentTenant.UserId!.Value)
                  .CountAsync(cancellationToken);
            if (count <= 0)
            {
                return Result<OffSetPagedList<Response>>.Failure(
                    VisitErrors.VisitsNotFound);
            }
            var visit = _db.Visits
                .ForTenant(_currentTenant.UserId!.Value)
                .AsNoTracking();
            if (!string.IsNullOrWhiteSpace(query.search))
            {
                visit = visit.Where(v =>
                    v.Animal.Name.Contains(query.search) ||
                    v.Owner.FullName.Contains(query.search));
            }
            var visits = visit
                         .Select(e => new Response(
                                e.Id,
                                e.VisitType.ToString(),
                                e.AnimalId,
                                e.Animal.Name,
                                e.Animal.Species,
                                e.OwnerId,
                                e.Owner.FullName,
                                e.CreatedOnUtc,
                                e.PaymentAmount,
                                e.PaymentStatus.ToString()));
            Expression<Func<Response, object>> orderSelector = query.SortColumn?
                            .ToLower() switch
            {
                "animalname" => e => e.AnimalName,
                "ownername" => e => e.OwnerName,
                "visitdate" => e => e.VisitDate,
                _ => e => e.VisitDate
            };
            var temp = await visits.ToListAsync(cancellationToken);
            if (temp is null)
            {
                return Result<OffSetPagedList<Response>>
                    .Failure(VisitErrors.VisitsNotFound);
            }

            var visitsQuery = temp.AsQueryable();
            if (query.SortOrder is "desc")
            {
                visitsQuery = visitsQuery.OrderByDescending(orderSelector);
            }
            else
            {
                visitsQuery = visitsQuery.OrderBy(orderSelector);
            }

            visitsQuery = visitsQuery.Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var data = visitsQuery.ToList();
            if (data is null)
            {
                return Result<OffSetPagedList<Response>>
                    .Failure(VisitErrors.VisitsNotFound);
            }
            var result = OffSetPagedList<Response>
                .Create(data, count, query.Page, query.PageSize);
            return Result<OffSetPagedList<Response>>.Success(result);
        }
    }

    /// <summary>Carter endpoint that maps <c>GET /visits</c>. Requires authorization.</summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the get-visits-table route with pagination and search query parameters.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/visits", [Authorize] async (
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                [FromQuery] string? sortColumn,
                [FromQuery] string? sortOrder,
                [FromQuery] string? search,
                IQueryHandler<TableRequest<Response>, OffSetPagedList<Response>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = TableRequest<Response>.Create(
                    pageSize,
                    page,
                    search,
                    sortColumn,
                    sortOrder
                    );
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();
            })
            .WithTags($"{nameof(Visit)}s")
            .WithSummary("Get all visits")
            .WithDescription("Retrieves a paginated list of all visits with optional search, sorting, and filtering capabilities.");
        }
    }
}
