using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Dashboard;

/// <summary>
/// Vertical slice for the dashboard summary cards endpoint.
/// Aggregates four counts (animals, appointments, visits, completed appointments) in a single request.
/// </summary>
public static class Dashboard
{
    /// <summary>Empty marker query for the dashboard statistics request.</summary>
    public sealed record Query : IQuery<Response>;

    /// <summary>Response DTO with four aggregated counts used to populate dashboard cards.</summary>
    /// <param name="TotalAnimals">Total number of animals in the tenant's clinic.</param>
    /// <param name="TotalAppointments">Total number of appointments in the tenant's clinic.</param>
    /// <param name="TotalVisits">Total number of visits in the tenant's clinic.</param>
    /// <param name="CompletedAppointments">Number of appointments with status <c>Completed</c>.</param>
    public sealed record Response(
        int TotalAnimals,
        int TotalAppointments,
        int TotalVisits,
        int CompletedAppointments);

    /// <summary>
    /// Handles the <see cref="Query"/> by executing four tenant-scoped count queries
    /// and returning the aggregated response.
    /// </summary>
    public sealed class DashboardQuery : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        /// <summary>Initializes the handler with database and tenant context services.</summary>
        public DashboardQuery(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        /// <summary>Runs four <c>CountAsync</c> calls scoped to the current tenant and composes the response.</summary>
        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken = default)
        {
            var totalAnimals = await _db.Animals
                .ForTenant(_currentTenant.UserId!.Value)
                .CountAsync(cancellationToken);
            var totalAppointments = await _db.Appointments
                .ForTenant(_currentTenant.UserId!.Value)
                .CountAsync(cancellationToken);
            var totalVisits = await _db.Visits
                .ForTenant(_currentTenant.UserId!.Value)
                .CountAsync(cancellationToken);
            var completedAppointments = await _db.Appointments
                .ForTenant(_currentTenant.UserId!.Value)
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
    /// <summary>Carter endpoint that maps <c>GET /dashboard/cards</c>. Requires authorization.</summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the dashboard statistics route.</summary>
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
