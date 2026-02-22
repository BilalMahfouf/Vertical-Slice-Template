using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;
using System.Linq.Expressions;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations.OffSet;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Clinics;

/// <summary>
/// Vertical slice for retrieving a paginated, filterable, and sortable list of clinics
/// belonging to the currently authenticated tenant.
/// </summary>
public static class GetAllClinics
{
    /// <summary>Read-model DTO representing a single clinic row in the results table.</summary>
    public record Response(
        Guid Id,
        Guid DoctorId,
        string DoctorName,
        string ClinicName,
        string Phone,
        string Address,
        int StaffCount,
        DateTime CreatedOnUtc);

    /// <summary>
    /// Handles <see cref="TableRequest{T}"/> for the clinics list.
    /// Applies tenant scoping, optional full-text search, in-memory column sort, and offset pagination.
    /// ⚠️ Warning: sorting is applied in-memory after <c>ToListAsync</c> — avoid for large datasets.
    /// </summary>
    public class GetAllClinicsQueryHandler
        : IQueryHandler<TableRequest<Response>, OffSetPagedList<Response>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        /// <summary>Initializes the handler with database and tenant context services.</summary>
        public GetAllClinicsQueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        /// <summary>
        /// Executes the paginated clinics query:
        /// <list type="number">
        ///   <item>Scopes to the current tenant via <c>ForTenant()</c>.</item>
        ///   <item>Applies optional search filter against clinic name, phone, and doctor name.</item>
        ///   <item>Sorts results in-memory by the requested column.</item>
        ///   <item>Paginates via <c>Skip/Take</c>.</item>
        /// </list>
        /// </summary>
        /// <param name="query">Pagination, search, and sort parameters.</param>
        /// <param name="cancellationToken">Token for cooperative cancellation.</param>
        /// <returns>A paginated list of clinic DTOs, or <c>ClinicErrors.ClinicsNotFound</c>.</returns>
        public async Task<Result<OffSetPagedList<Response>>> Handle(
            TableRequest<Response> query,
            CancellationToken cancellationToken = default)
        {
            var count = await _db.Clinics
                .ForTenant(_currentTenant.UserId!.Value)
                .CountAsync(cancellationToken);
            if (count <= 0)
            {
                return Result<OffSetPagedList<Response>>.Failure(
                    ClinicErrors.ClinicsNotFound);
            }
            var clinics = _db.Clinics
                .ForTenant(_currentTenant.UserId!.Value)
                .Select(e => new Response(
                    e.Id,
                    e.DoctorId,
                    e.Doctor.FullName,
                    e.Name,
                    e.Phone,
                    e.Address,
                    e.StaffCount,
                    e.CreatedOnUtc));
            if (!string.IsNullOrWhiteSpace(query.search))
            {
                clinics = clinics.Where(e => e.ClinicName.ToLower().Contains(query.search)
                || e.Phone.ToLower().Contains(query.search)
                || e.DoctorName.ToLower().Contains(query.search));
            }
            var temp = await clinics.ToListAsync(cancellationToken);
            if (temp is null)
            {
                return Result<OffSetPagedList<Response>>.Failure(ClinicErrors
                    .ClinicsNotFound);
            }
            var tempQuery = temp.AsQueryable();
            Expression<Func<Response, object>> orderSelector = query.SortColumn?
                .ToLower() switch
            {
                "doctorid" => e => e.DoctorId,
                "doctorname" => e => e.DoctorName,
                "clinicname" => e => e.ClinicName,
                "phone" => e => e.Phone,
                "staffcount" => e => e.StaffCount,
                _ => e => e.CreatedOnUtc
            };
            if (query.SortOrder is "desc")
            {
                tempQuery = tempQuery.OrderByDescending(orderSelector);
            }
            else
            {
                tempQuery = tempQuery.OrderBy(orderSelector);
            }
            tempQuery = tempQuery.Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var data = tempQuery.ToList();
            if (data is null)
            {
                return Result<OffSetPagedList<Response>>
                    .Failure(ClinicErrors.ClinicsNotFound);
            }
            var result = OffSetPagedList<Response>
                .Create(data, count, query.Page, query.PageSize);
            return Result<OffSetPagedList<Response>>.Success(result);
        }
    }
    /// <summary>
    /// Carter endpoint that maps <c>GET /clinics</c>.
    /// Accepts optional query parameters for pagination, search, and sorting.
    /// Requires authorization.
    /// </summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the get-all-clinics route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/clinics", [Authorize] async (
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                [FromQuery] string? sortColumn,
                [FromQuery] string? sortOrder,
                [FromQuery] string? search,
                [FromServices] IQueryHandler<TableRequest<Response>, OffSetPagedList<Response>> handler,
                CancellationToken cancellationToken) =>
            {
                TableRequest<Response> query = TableRequest<Response>
                .Create(pageSize, page, search, sortColumn, sortOrder);
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value)
                : result.Problem();

            })
            .WithTags($"{nameof(Clinic)}s")
            .WithSummary("Get all clinics")
            .WithDescription("Retrieves a paginated list of all clinics with optional search, sorting, and filtering capabilities.");
        }
    }
}
