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

namespace VeterinaryApi.Features.Clinics;

public static class GetAllClinics
{


    public record Response(
        Guid Id,
        Guid DoctorId,
        string DoctorName,
        string ClinicName,
        string Phone,
        string Address,
        int StaffCount,
        DateTime CreatedOnUtc);
    public class GetAllClinicsQueryHandler
        : IQueryHandler<TableRequest<Response>, OffSetPagedList<Response>>
    {
        private readonly IApplicationDbContext _db;

        public GetAllClinicsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result<OffSetPagedList<Response>>> Handle(
            TableRequest<Response> query,
            CancellationToken cancellationToken = default)
        {
            var count = await _db.Clinics.CountAsync(cancellationToken);
            if (count <= 0)
            {
                return Result<OffSetPagedList<Response>>.Failure(
                    ClinicErrors.ClinicsNotFound);
            }
            var clinics = _db.Clinics
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
                _ => e => e.Id
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
    public class Endpoint : IEndpoint
    {
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
