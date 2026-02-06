using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Appointments;

namespace VeterinaryApi.Features.Appointments;

public static class GetAllAppointments
{
    public record Response(
        Guid Id,
        Guid ClinicId,
        Guid ClientId,
        string ClientName,
        string AnimalName,
        DateTime AppointmentDate,
        string Status,
        DateTime CreatedOnUtc);

    public class GetAllAppointmentsQueryHandler
        : IQueryHandler<TableRequest<Response>, PagedList<Response>>
    {
        private readonly IApplicationDbContext _db;

        public GetAllAppointmentsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result<PagedList<Response>>> Handle(
            TableRequest<Response> query,
            CancellationToken cancellationToken = default)
        {
            var count = await _db.Appointments.CountAsync(cancellationToken);
            if (count <= 0)
            {
                return Result<PagedList<Response>>.Failure(
                    AppointmentErrors.AppointmentsNotFound);
            }
            var appointment = _db.Appointments.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(query.search))
            {
                appointment = appointment.Where(e =>
                    e.Animal.Client.FullName.ToLower().Contains(query.search) ||
                    e.Animal.Name.ToLower().Contains(query.search) ||
                    e.Status.ToString().ToLower().Contains(query.search));
            }

            var appointments = appointment.Select(e => new Response(
                                            e.Id,
                                            e.ClinicId,
                                            e.Animal.ClientId,
                                            e.Animal.Client.FullName,
                                            e.Animal.Name,
                                            e.AppointmentDate,
                                            e.Status.ToString(),
                                            e.CreatedOnUtc));


            Expression<Func<Response, object>> orderSelector = query.SortColumn?
                .ToLower() switch
            {
                "date" => e => e.AppointmentDate,
                "status" => e => e.Status,
                "clientname" => e => e.ClientName,
                _ => e => e.Id
            };
            var temp = await appointments.ToListAsync(cancellationToken);
            if (temp is null)
            {
                return Result<PagedList<Response>>
                    .Failure(AppointmentErrors.AppointmentsNotFound);
            }

            var appointmentsQuery = temp.AsQueryable();
            if (query.SortOrder is "desc")
            {
                appointmentsQuery = appointmentsQuery.OrderByDescending(orderSelector);
            }
            else
            {
                appointmentsQuery = appointmentsQuery.OrderBy(orderSelector);
            }

            appointmentsQuery = appointmentsQuery.Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var data = appointmentsQuery.ToList();
            if (data is null)
            {
                return Result<PagedList<Response>>
                    .Failure(AppointmentErrors.AppointmentsNotFound);
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
            app.MapGet("/appointments", [Authorize] async (
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                [FromQuery] string? sortColumn,
                [FromQuery] string? sortOrder,
                [FromQuery] string? search,
                [FromServices] IQueryHandler<TableRequest<Response>, PagedList<Response>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = TableRequest<Response>
                    .Create(pageSize, page, search, sortColumn, sortOrder);
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();

            })
            .WithTags($"{nameof(Appointment)}s")
            .WithSummary("Get all appointments")
            .WithDescription("Retrieves a paginated list of all appointments with optional search, sorting, and filtering capabilities.");
        }
    }
}
