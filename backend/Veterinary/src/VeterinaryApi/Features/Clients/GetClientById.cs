using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clients;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Clients;

/// <summary>
/// Vertical slice for retrieving a single client by unique identifier, scoped to the current tenant.
/// </summary>
public static class GetClientById
{
    /// <summary>Query carrying the target client identifier.</summary>
    /// <param name="Id">The unique identifier of the client to retrieve.</param>
    public record Query(Guid Id) : IQuery<Response>;

    /// <summary>Read-model DTO for a client detail lookup.</summary>
    public record Response(
        Guid Id,
        Guid ClinicId,
        string ClinicName,
        string FullName,
        string Phone,
        string? Notes);

    /// <summary>Handles the <see cref="Query"/> with a tenant-scoped, non-tracked projection.</summary>
    public class GetClientByIdQueryHandler : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        /// <summary>Initializes the handler with database and tenant context services.</summary>
        public GetClientByIdQueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }

        /// <summary>Projects the client and clinic to a <see cref="Response"/> DTO.</summary>
        /// <returns>A successful result, or <c>ClientErrors.ClientNotFound</c>.</returns>
        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var client = await _db.Clients
                .ForTenant(_currentTenant.UserId!.Value)
                .AsNoTracking()
                .Where(e => e.Id == query.Id)
                .Select(e => new Response(
                    e.Id,
                    e.ClinicId,
                    e.Clinic.Name,
                    e.FullName,
                    e.Phone,
                    e.Notes))
                .FirstOrDefaultAsync(cancellationToken);

            if (client is null)
            {
                return Result<Response>
                    .Failure(ClientErrors.ClientNotFound(query.Id));
            }
            return Result<Response>.Success(client);
        }
    }
    /// <summary>Carter endpoint that maps <c>GET /clients/{id}</c>. Requires authorization.</summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the get-client-by-id route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/clients/{id:guid}", [Authorize] async (
                Guid id,
                IQueryHandler<Query, Response> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new Query(id);
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();
            })
            .WithTags($"{nameof(Client)}s")
            .WithSummary("Get client by ID")
            .WithDescription("Retrieves detailed information about a specific client by their unique identifier.");
        }
    }
}