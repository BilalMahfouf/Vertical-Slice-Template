using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Paginations.OffSet;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Appointments;

/// <summary>
/// Vertical slice for retrieving a paginated, searchable, sortable list of appointments
/// belonging to the currently authenticated tenant (doctor).
/// </summary>
public static class GetAllAppointments
{
    /// <summary>
    /// Appointment summary DTO projected from the database for list views.
    /// </summary>
    /// <param name="Id">The appointment's unique identifier.</param>
    /// <param name="ClinicId">The clinic at which the appointment is scheduled.</param>
    /// <param name="ClientId">The animal owner's (client's) unique identifier.</param>
    /// <param name="ClientName">Full name of the animal owner.</param>
    /// <param name="AnimalName">Name of the animal being seen.</param>
    /// <param name="AppointmentDate">The scheduled date and time.</param>
    /// <param name="Status">The stringified appointment status (e.g., "Confirmed").</param>
    /// <param name="CreatedOnUtc">The UTC timestamp when the appointment was created.</param>
    public record Response(
        Guid Id,
        Guid ClinicId,
        Guid ClientId,
        string ClientName,
        string AnimalName,
        DateTime AppointmentDate,
        string Status,
        DateTime CreatedOnUtc);

    /// <summary>
    /// Query handler that retrieves a paginated, optionally filtered and sorted page of appointments
    /// for the currently-authenticated tenant.
    /// </summary>
    /// <remarks>
    /// <b>Known performance issue:</b> Sorting is applied in-memory after fetching all matching
    /// rows from the database. For large appointment datasets, consider pushing the
    /// <c>OrderBy</c>/<c>OrderByDescending</c> expression into the EF Core LINQ query before
    /// calling <c>ToListAsync</c>.
    /// </remarks>
    public class GetAllAppointmentsQueryHandler
        : IQueryHandler<TableRequest<Response>, OffSetPagedList<Response>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        /// <summary>Initializes the handler with database context and tenant accessor.</summary>
        public GetAllAppointmentsQueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        /// <summary>
        /// Handles the paginated appointments query:
        /// <list type="number">
        ///   <item>Counts all appointments for the tenant.</item>
        ///   <item>Applies optional search filter (client name, animal name, status).</item>
        ///   <item>Projects to <see cref="Response"/> DTOs.</item>
        ///   <item>Applies sorting and pagination in-memory.</item>
        /// </list>
        /// </summary>
        /// <param name="query">Paging, sorting, and search parameters wrapped in <see cref="TableRequest{T}"/>.</param>
        /// <param name="cancellationToken">Token for cooperative cancellation.</param>
        /// <returns>
        /// A successful paginated result, or <c>AppointmentErrors.AppointmentsNotFound</c>
        /// if the tenant has no appointments.
        /// </returns>
        public async Task<Result<OffSetPagedList<Response>>> Handle(
            TableRequest<Response> query,
            CancellationToken cancellationToken = default)
        {
            var count = await _db.Appointments
                .ForTenant(_currentTenant.UserId!.Value)
                .CountAsync(cancellationToken);

            if (count <= 0)
            {
                return Result<OffSetPagedList<Response>>.Failure(
                    AppointmentErrors.AppointmentsNotFound);
            }

            var appointment = _db.Appointments
                .ForTenant(_currentTenant.UserId!.Value)
                .AsNoTracking();

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
                _ => e => e.CreatedOnUtc
            };

            // TODO: Push sorting into the database query to avoid loading all rows in memory.
            var temp = await appointments.ToListAsync(cancellationToken);
            if (temp is null)
            {
                return Result<OffSetPagedList<Response>>
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
                return Result<OffSetPagedList<Response>>
                    .Failure(AppointmentErrors.AppointmentsNotFound);
            }

            var result = OffSetPagedList<Response>
                .Create(data, count, query.Page, query.PageSize);
            return Result<OffSetPagedList<Response>>.Success(result);
        }
    }

    /// <summary>
    /// Carter endpoint that maps <c>GET /appointments</c> to the paginated appointments handler.
    /// Accepts optional query-string parameters for page, size, sort column, sort order, and search.
    /// Returns <c>200 OK</c> with a <see cref="OffSetPagedList{T}"/> or a Problem Details error response.
    /// </summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the <c>GET /appointments</c> route with pagination parameters.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/appointments", [Authorize] async (
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
            .WithTags($"{nameof(Appointment)}s")
            .WithSummary("Get all appointments")
            .WithDescription("Retrieves a paginated list of all appointments with optional search, sorting, and filtering capabilities.");
        }
    }
}
