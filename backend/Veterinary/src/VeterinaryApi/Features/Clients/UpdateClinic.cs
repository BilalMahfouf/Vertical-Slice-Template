using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clients;

namespace VeterinaryApi.Features.Clients;

public static class UpdateClient
{
    public record Request(
        string FirstName,
        string LastName,
        string Phone,
        string? Notes);
    public record UpdateClientCommand(
        Guid Id,
        string FirstName,
        string LastName,
        string Phone,
        string? Notes) : ICommand;

    public class UpdateClientCommandHandler : ICommandHandler<UpdateClientCommand>
    {
        private readonly IApplicationDbContext _db;
        public UpdateClientCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Result> Handle(
            UpdateClientCommand command,
            CancellationToken cancellationToken)
        {
            var client = await _db.Clients
                .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

            if (client is null)
            {
                return Result.Failure(ClientErrors.ClientNotFound(command.Id));
            }

            client.UpdateDetails(
                command.FirstName,
                command.LastName,
                command.Phone,
                command.Notes);

            _db.Clients.Update(client);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/clients/{id:guid}",[Authorize] async (
                Guid id,
                [FromBody] Request request,
                ICommandHandler<UpdateClientCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateClientCommand(
                    id,
                    request.FirstName,
                    request.LastName,
                    request.Phone,
                    request.Notes);
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.NoContent() :
                    result.Problem();
            }).WithTags("clients");
        }
    }
}