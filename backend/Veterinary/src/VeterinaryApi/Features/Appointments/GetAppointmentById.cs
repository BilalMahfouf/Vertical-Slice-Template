using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Appointments;

namespace VeterinaryApi.Features.Appointments;

public static class GetAppointmentById
{
    public record Query(Guid Id) : IQuery<Response>;
    public record Response(
        Guid Id,
        Guid ClinicId,
        Guid ClientId,
        string ClientName,
        string AnimalName,
        DateTime AppointmentDate,
        string Status,
        DateTime CreatedOnUtc);

    public class GetAppointmentByIdQueryHandler : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _db;
        public GetAppointmentByIdQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var appointment = await _db.Appointments
                .AsNoTracking()
                .Where(e => e.Id == query.Id)
                .Select(e => new Response(
                    e.Id,
                    e.ClinicId,
                    e.Animal.ClientId,
                    e.Animal.Client.FullName,
                    e.Animal.Name,
                    e.AppointmentDate,
                    e.Status.ToString(),
                    e.CreatedOnUtc))
                .FirstOrDefaultAsync(cancellationToken);

            if (appointment is null)
            {
                return Result<Response>
                    .Failure(AppointmentErrors.AppointmentNotFound(query.Id));
            }
            return Result<Response>.Success(appointment);
        }
    }
    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/appointments/{id:guid}",[Authorize] async (
                Guid id,
                IQueryHandler<Query, Response> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new Query(id);
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();
            }).WithTags("appointments");
        }
    }
}
