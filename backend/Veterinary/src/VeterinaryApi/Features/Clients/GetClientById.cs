using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clients;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Clients;

public static class GetClientById
{
    public record Query(Guid Id) : IQuery<Response>;
    public record Response(
        Guid Id,
        Guid ClinicId,
        string ClinicName,
        string FullName,
        string Phone,
        string? Notes);

    public class GetClientByIdQueryHandler : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        public GetClientByIdQueryHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }
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
    public class Endpoint : IEndpoint
    {
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