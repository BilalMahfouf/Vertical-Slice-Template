using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Appointments;

namespace VeterinaryApi.Features.Dashboard;

public static class Dashboard
{
    public sealed record Query : IQuery<Response>;
    public sealed record Response(
        int TotalAnimals,
        int TotalAppointments,
        int TotalVisits,
        int CompletedAppointments);

    public sealed class DashboardQuery : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _db;

        public DashboardQuery(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken = default)
        {
            var totalAnimals = await _db.Animals.CountAsync(cancellationToken);
            var totalAppointments = await _db.Appointments.CountAsync(cancellationToken);
            var totalVisits = await _db.Visits.CountAsync(cancellationToken);
            var completedAppointments = await _db.Appointments
                .CountAsync(e => e.Status == AppointmentStatus.Completed,
                cancellationToken);

            var response = new Response(
                TotalAnimals: totalAnimals,
                TotalAppointments: totalAppointments,
                TotalVisits: totalVisits,
                completedAppointments);
            return Result<Response>.Success(response);
        }
    }
    public sealed class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/dashboard/cards", [Authorize] async (
                IQueryHandler<Query, Response> handler,
                CancellationToken cancellationToken = default) =>
            {
                var query = new Query();
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value)
                : result.Problem();
            })
            .WithTags(nameof(Dashboard))
            .WithSummary("Get dashboard statistics")
            .WithDescription("Retrieves summary statistics for the dashboard including total animals, appointments, visits, and completed appointments count.");
        }
    }
}
