using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clients;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Clients;

/// <summary>
/// Vertical slice for looking up a client by partial full-name match, scoped to the current tenant.
/// </summary>
public static class GetClientByName
{
    /// <summary>Query carrying the name string to search for.</summary>
    /// <param name="Name">Partial or full client name to match against <c>FullName</c>.</param>
    public sealed record Query(string Name) : IQuery<ClientReadResponse>;

    /// <summary>FluentValidation validator ensuring the name is non-empty.</summary>
    public sealed class Validator : AbstractValidator<Query>
    {
        /// <summary>Configures the name validation rule.</summary>
        public Validator()
        {
            RuleFor(e => e.Name).NotEmpty();
        }
    }

    /// <summary>Handles the <see cref="Query"/> by searching for the first matching client name.</summary>
    public sealed class GetClientByNameQueryHandler
        : IQueryHandler<Query, ClientReadResponse>
    {
        private readonly IApplicationDbContext _db;
        private readonly IValidator<Query> _validator;
        private readonly ICurrentTenant _currentTenant;

        /// <summary>Initializes the handler with database, validator, and tenant context services.</summary>
        public GetClientByNameQueryHandler(
            IApplicationDbContext db,
            IValidator<Query> validator,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _validator = validator;
            _currentTenant = currentTenant;
        }

        /// <summary>
        /// Validates the query, then returns the first client whose <c>FullName</c> contains
        /// the search string (case-sensitive DB collation applies).
        /// </summary>
        /// <returns>A successful result with the matching client, or <c>ClientErrors.ClientNotFound</c>.</returns>
        public async Task<Result<ClientReadResponse>> Handle(
            Query query,
            CancellationToken cancellationToken = default)
        {
            _validator.ValidateAndThrow(query);

            var client = await _db.Clients
                .ForTenant(_currentTenant.UserId!.Value)
                .Where(e => e.FullName.Contains(query.Name))
                .Select(e => new ClientReadResponse(
                    e.Id,
                    e.ClinicId,
                    e.Clinic.Name,
                    e.FullName,
                    e.Phone,
                    e.Notes,
                    e.CreatedOnUtc,
                    e.Animals.Count)
                )
            .FirstOrDefaultAsync();
            if (client is null)
            {
                return Result<ClientReadResponse>.Failure(
                    ClientErrors.ClientNotFound());
            }
            return Result<ClientReadResponse>.Success(client);
        }
    }
    /// <summary>Carter endpoint that maps <c>GET /clients/by-name/{name}</c>.</summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the get-client-by-name route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/clients/by-name/{name}", async (
                string name,
                IQueryHandler<Query, ClientReadResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new Query(name);
                var result = await handler
                    .Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) :
                    result.Problem();
            })
            .WithTags($"{nameof(Client)}s")
            .WithSummary("Get client by name")
            .WithDescription("Searches for a client by their full name. Returns the first matching client.");
        }
    }
}
