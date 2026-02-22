using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Clinics;

/// <summary>
/// Vertical slice exposing the clinic associated with the authenticated user.
/// The route parameter <c>userId</c> is currently ignored —
/// the authenticated user's ID from <see cref="ICurrentTenant"/> is used instead.
/// </summary>
public static class GetClinicByUserId
{
    /// <summary>
    /// Carter endpoint that maps <c>GET /clinics/user/{userId}</c>.
    /// Resolves the clinic for the current tenant and delegates to <see cref="GetClinicById"/>.
    /// Requires authorization.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the get-clinic-by-user route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/clinics/user/{userId:guid}", async (
                Guid userId,
               [FromServices] IApplicationDbContext db,
               [FromServices] ICurrentTenant currentTenant,
               [FromServices] IQueryHandler<GetClinicById.Query
               ,GetClinicById.Response> handler,
                CancellationToken ct) =>
            {
                var clinicId = await db.Clinics
                .ForTenant(currentTenant.UserId!.Value)
                .Where(e => e.DoctorId == currentTenant.UserId)
                .Select(e => e.Id)
                .FirstOrDefaultAsync(ct);

                var result = await handler
                .Handle(new GetClinicById.Query(clinicId), ct);

                return result.IsSuccess ? Results.Ok(result.Value)
                : result.Problem();

            }).WithTags($"{nameof(Clinic)}s")
            .RequireAuthorization()
            .WithSummary("Get clinic by user ID")
            .WithDescription("Retrieves the clinic associated with the authenticated user. Note: The route parameter is currently ignored in favor of the authenticated user's ID.");
        }
    }
}
