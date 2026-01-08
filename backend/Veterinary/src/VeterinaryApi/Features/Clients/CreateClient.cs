using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clients;
using VeterinaryApi.Domain.Clinics;

namespace VeterinaryApi.Features.Clients;

public static class CreateClient
{
    public record Command(
        string firstName,
        string lastName,
        string phone,
        string? notes) : ICommand<Response>;

    public record Response(Guid Id);

    public class CommandHandler
        : ICommandHandler<Command, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUser _currentUser;


        public CommandHandler(
            IApplicationDbContext db,
            ICurrentUser currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<Result<Response>> Handle(
            Command command,
            CancellationToken cancellationToken = default)
        {
            var clinic = await _db.Clinics
                .Where(e => e.DoctorId == _currentUser.UserId)
                .Select(e => new { e.Id })
                .FirstOrDefaultAsync(cancellationToken);
            if (clinic is null)
            {
                return Result<Response>.Failure(
                    ClinicErrors.ClinicNotFound());
            }
            var client = Client.Create(
                clinic.Id,
                command.firstName,
                command.lastName,
                command.phone,
                command.notes);
            _db.Clients.Add(client);
            await _db.SaveChangesAsync(cancellationToken);
            return Result<Response>.Success(new Response(client.Id));
        }
    }
    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/clients", [Authorize] async (
                [FromBody] Command command,
                [FromServices] ICommandHandler<Command, Response> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.Created("/", result.Value)
                : result.Problem();
            }).WithTags("clients");
        }
    }
}
