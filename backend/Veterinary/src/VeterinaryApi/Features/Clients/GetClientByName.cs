using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clients;

namespace VeterinaryApi.Features.Clients;

public static class GetClientByName
{
    public sealed record Query(string Name) : IQuery<ClientReadResponse>;

    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(e => e.Name).NotEmpty();
        }
    }

    public sealed class GetClientByNameQueryHandler
        : IQueryHandler<Query, ClientReadResponse>
    {
        private readonly IApplicationDbContext _db;
        private readonly IValidator<Query> _validator;

        public GetClientByNameQueryHandler(
            IApplicationDbContext db,
            IValidator<Query> validator)
        {
            _db = db;
            _validator = validator;
        }

        public async Task<Result<ClientReadResponse>> Handle(
            Query query,
            CancellationToken cancellationToken = default)
        {
            _validator.ValidateAndThrow(query);

            var client = await _db.Clients
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
    public sealed class Endpoint : IEndpoint
    {
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
