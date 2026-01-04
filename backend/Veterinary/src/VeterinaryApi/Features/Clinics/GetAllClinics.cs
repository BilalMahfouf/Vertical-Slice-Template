using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;
using System.Linq.Expressions;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clinics;

namespace VeterinaryApi.Features.Clinics;

public static class GetAllClinics
{

    public class Request : TableRequest, IQuery<PagedList<Response>>;

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
        : IQueryHandler<Request, PagedList<Response>>
    {
        private readonly IApplicationDbContext _db;

        public GetAllClinicsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result<PagedList<Response>>> Handle(
            Request query,
            CancellationToken cancellationToken = default)
        {
            var count = await _db.Clinics.CountAsync(cancellationToken);
            if (count <= 0)
            {
                return Result<PagedList<Response>>.Failure(
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
                clinics = clinics.OrderByDescending(orderSelector);
            }
            else
            {
                clinics = clinics.OrderBy(orderSelector);
            }
            clinics = clinics.Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var data = await clinics.ToListAsync(cancellationToken);
            if (data is null)
            {
                return Result<PagedList<Response>>
                    .Failure(ClinicErrors.ClinicsNotFound);
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
            app.MapGet("/clinics", async (
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                [FromQuery] string? sortColumn,
                [FromQuery] string? sortOrder,
                [FromQuery] string? search,
                [FromServices] IQueryHandler<Request, PagedList<Response>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = (Request)TableRequest
                .Create(pageSize, page, search, sortColumn, sortOrder);
                var result = await handler.Handle(query, cancellationToken);

            }).WithTags("clinics");
        }
    }
}
