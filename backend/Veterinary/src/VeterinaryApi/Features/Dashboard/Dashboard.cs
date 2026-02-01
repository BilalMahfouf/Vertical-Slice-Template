using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;

namespace VeterinaryApi.Features.Dashboard;

public static class Dashboard
{
    public sealed record Query : IQuery<Response>;
    public sealed record Response(
        int TotalAnimals,
        int TotalAppointments,
        int TotalVisits);

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
            var totalAnimals = await _db.Animals.CountAsync();
            var totalAppointments = await _db.Appointments.CountAsync();
            var totalVisits = await _db.Visits.CountAsync();

            var response = new Response(
                TotalAnimals: totalAnimals,
                TotalAppointments: totalAppointments,
                TotalVisits: totalVisits);
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
            }).WithTags($"{nameof(Dashboard)}");
        }
    }
}
