using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Clinics;

/// <summary>
/// Vertical slice for retrieving a single clinic by its unique identifier,
/// scoped to the currently authenticated tenant.
/// </summary>
public static class GetClinicById
{
    /// <summary>
    /// Query carrying the target clinic's identifier.
    /// Implements <see cref="IQuery{TResponse}"/> where the response is <see cref="Response"/>.
    /// </summary>
    /// <param name="Id">The unique identifier of the clinic to retrieve.</param>
    public record Query(Guid Id) : IQuery<Response>;

    /// <summary>Read-model DTO for a clinic detail lookup.</summary>
    /// <param name="Id">Clinic unique identifier.</param>
    /// <param name="Name">Clinic name.</param>
    /// <param name="Phone">Clinic phone number.</param>
    /// <param name="Address">Clinic address.</param>
    public record Response(Guid Id, string Name, string Phone, string Address);

    /// <summary>
    /// Handles the <see cref="Query"/> with a tenant-scoped, non-tracked projection.
    /// </summary>
    public class GetClinicByIdQueryHandler : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        /// <summary>Initializes the handler with database and tenant context services.</summary>
        public GetClinicByIdQueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        /// <summary>
        /// Projects the clinic entity to a <see cref="Response"/> DTO using tenant scoping.
        /// </summary>
        /// <param name="query">The query with the target clinic ID.</param>
        /// <param name="cancellationToken">Token for cooperative cancellation.</param>
        /// <returns>A successful result with the clinic DTO, or <c>ClinicErrors.ClinicNotFound</c>.</returns>
        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var clinic = await _db.Clinics
                .ForTenant(_currentTenant.UserId!.Value)
                .AsNoTracking()
                .Where(e => e.Id == query.Id)
                .Select(e => new Response(
                    e.Id,
                    e.Name,
                    e.Phone,
                    e.Address))
                .FirstOrDefaultAsync(cancellationToken);
            if (clinic is null)
            {
                return Result<Response>
                    .Failure(ClinicErrors.ClinicNotFound(query.Id));
            }
            return Result<Response>.Success(clinic);
        }
    }
    /// <summary>
    /// Carter endpoint that maps <c>GET /clinics/{id}</c>.
    /// Requires authorization. Returns <c>200 OK</c> with the clinic DTO, or Problem Details.
    /// </summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the get-clinic-by-id route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/clinics/{id:guid}", [Authorize] async (
                Guid id,
                IQueryHandler<Query, Response> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new Query(id);
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();
            })
            .WithTags($"{nameof(Clinic)}s")
            .WithSummary("Get clinic by ID")
            .WithDescription("Retrieves detailed information about a specific clinic by its unique identifier.");
        }
    }
}
