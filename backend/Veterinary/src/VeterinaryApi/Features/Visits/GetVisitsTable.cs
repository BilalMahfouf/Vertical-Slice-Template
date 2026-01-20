using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Features.Visits;

public class GetVisitsTable
{

    public record Response(
    Guid Id,
    string VisitType,
    Guid AnimalId,
    string AnimalName,
    Guid OwnerId,
    string OwnerName,
    DateTime VisitDate);

    public class GetVisitsTableQueryHandler
        : IQueryHandler<TableRequest<Response>, PagedList<Response>>
    {
        private readonly IApplicationDbContext _db;

        public GetVisitsTableQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<Result<PagedList<Response>>>Handle(
            TableRequest<Response> query,
            CancellationToken cancellationToken = default)
        {
            var count = await _db.Visits
                  .CountAsync(cancellationToken);
            if (count <= 0)
            {
                return Result<PagedList<Response>>.Failure(
                    VisitErrors.VisitsNotFound);
            }
            var visit = _db.Visits
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
                                e.OwnerId,
                                e.Owner.FullName,
                                e.CreatedOnUtc));
            Expression<Func<Response, object>> orderSelector = query.SortColumn?
                            .ToLower() switch
            {
                "animalname" => e => e.AnimalName,
                "ownername" => e => e.OwnerName,
                "visitdate" => e => e.VisitDate,
                _ => e => e.Id
            };
            var temp = await visits.ToListAsync(cancellationToken);
            if (temp is null)
            {
                return Result<PagedList<Response>>
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
                return Result<PagedList<Response>>
                    .Failure(VisitErrors.VisitsNotFound);
            }
            var result = PagedList<Response>
                .Create(data, count, query.Page, query.PageSize);
            return Result<PagedList<Response>>.Success(result);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/clinics/{clinicId:guid}/visits", [Authorize] async (
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                [FromQuery] string? sortColumn,
                [FromQuery] string? sortOrder,
                [FromQuery] string? search,
                IQueryHandler<TableRequest<Response>, PagedList<Response>> handler,
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
            }).WithTags("visits");
        }
    }
}
