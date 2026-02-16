using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations.OffSet;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Vaccinations;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Vaccinations;

/// <summary>
/// Feature for retrieving vaccines in a paginated table format.
/// </summary>
public static class GetVaccinationTable
{
    /// <summary>
    /// Response containing vaccination record information for table display.
    /// </summary>
    /// <param name="Id">The unique identifier of the vaccination.</param>
    /// <param name="VaccinationName">The name of the vaccination administered.</param>
    /// <param name="GivenAt">The date and time when the vaccination was administered.</param>
    /// <param name="DueTo">The date and time when the next vaccination is due.</param>
    /// <param name="ClientName">The full name of the client (animal owner).</param>
    /// <param name="AnimalName">The name of the vaccinated animal.</param>
    /// <param name="CreatedOnUtc">The date and time when the vaccination record was created.</param>
    public sealed record Response(
        Guid Id,
        string VaccinationName,
        DateTime GivenAt,
        DateTime? DueTo,
        string ClientName,
        string AnimalName,
        DateTime CreatedOnUtc);

    /// <summary>
    /// Handler for retrieving paginated vaccination records.
    /// Supports searching, sorting, and filtering of vaccination data.
    /// </summary>
    public sealed class QueryHandler
        : IQueryHandler<TableRequest<Response>, OffSetPagedList<Response>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        public QueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        public async Task<Result<OffSetPagedList<Response>>> Handle(
            TableRequest<Response> query,
            CancellationToken cancellationToken = default)
        {
            var count = await _db.Vaccinations
                .ForTenant(_currentTenant.UserId!.Value)
                .CountAsync(cancellationToken);
            if (count <= 0)
            {
                return Result<OffSetPagedList<Response>>
                    .Failure(VaccinationErrors.NotFound);
            }

            var vaccinationQuery = _db.Vaccinations.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.search))
            {
                vaccinationQuery = vaccinationQuery
                    .Where(e => e.Name.ToLower().Contains(query.search)
                    || e.Animal.Name.ToLower().Contains(query.search));
            }
            var vaccinations = vaccinationQuery
                .Select(e => new Response(
                    e.Id,
                    e.Name,
                    e.GivenAt,
                    e.DueTo,
                    e.Animal.Client.FullName,
                    e.Animal.Name,
                    e.CreatedOnUtc));
            Expression<Func<Response, object>> orderSelector = query.SortColumn?
                         .ToLower() switch
            {
                "name" => e => e.VaccinationName,
                "givenat" => e => e.GivenAt,
                "dueto" => e => e.DueTo ?? e.CreatedOnUtc,
                "clientname" => e => e.ClientName,
                _ => e => e.CreatedOnUtc
            };
            var temp = await vaccinations.ToListAsync(cancellationToken);
            if (temp is null)
            {
                return Result<OffSetPagedList<Response>>
                   .Failure(VaccinationErrors.NotFound);
            }
            var vaccinationsQuery = temp.AsQueryable();
            if (query.SortOrder is "desc")
            {
                vaccinationsQuery = vaccinationsQuery.OrderByDescending(orderSelector);
            }
            else
            {
                vaccinationsQuery = vaccinationsQuery.OrderBy(orderSelector);
            }
            vaccinationsQuery = vaccinationsQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);
            var data =  vaccinationsQuery.ToList();
            if (data is null)
            {
                return Result<OffSetPagedList<Response>>
                    .Failure(VaccinationErrors.NotFound);
            }
            var response = OffSetPagedList<Response>
                .Create(data, count, query.Page, query.PageSize);

            return Result<OffSetPagedList<Response>>
                    .Success(response);
        }
    }

    /// <summary>
    /// Endpoint configuration for retrieving vaccination records in table format.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>
        /// Registers the GET /vaccinations endpoint.
        /// </summary>
        /// <remarks>
        /// Retrieves a paginated list of vaccination records with optional search, sorting, and filtering.
        /// Requires authorization.
        /// Returns 200 OK with paginated vaccination data or a problem response on failure.
        /// </remarks>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/vaccinations", async (
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                [FromQuery] string? search,
                [FromQuery] string? sortColumn,
                [FromQuery] string? sortOrder,
                IQueryHandler<TableRequest<Response>, OffSetPagedList<Response>> handler,
                CancellationToken ct = default) =>
            {
                var query = TableRequest<Response>
                .Create(pageSize, page, search, sortColumn, sortOrder);

                var result = await handler.Handle(query, ct);

                return result.IsSuccess ? Results.Ok(result.Value)
                : result.Problem();

            })
            .WithTags($"{nameof(Vaccination)}s")
            .WithSummary("Get all vaccinations")
            .WithDescription("Retrieves a paginated list of all vaccination records with optional search, sorting, and filtering capabilities.")
            .RequireAuthorization();
        }
    }
}
