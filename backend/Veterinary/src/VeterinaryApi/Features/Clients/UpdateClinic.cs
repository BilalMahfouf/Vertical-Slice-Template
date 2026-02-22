using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clients;

namespace VeterinaryApi.Features.Clients;

/// <summary>
/// Vertical slice for updating an existing client's contact details.
/// Note: the file is named <c>UpdateClinic.cs</c> but the contained class is <c>UpdateClient</c> — a naming inconsistency.
/// </summary>
public static class UpdateClient
{
    /// <summary>HTTP request body DTO for the update-client endpoint.</summary>
    public record Request(
        string FirstName,
        string LastName,
        string Phone,
        string? Notes);

    /// <summary>Command carrying the updated client values plus the target ID.</summary>
    public record UpdateClientCommand(
        Guid Id,
        string FirstName,
        string LastName,
        string Phone,
        string? Notes) : ICommand;

    /// <summary>Handles the <see cref="UpdateClientCommand"/> by loading the client and applying updates.</summary>
    public class UpdateClientCommandHandler : ICommandHandler<UpdateClientCommand>
    {
        private readonly IApplicationDbContext _db;

        /// <summary>Initializes the handler with the application database context.</summary>
        public UpdateClientCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Loads the client, calls <c>Client.UpdateDetails()</c>, and persists.</summary>
        /// <returns>A successful result, or <c>ClientErrors.ClientNotFound</c>.</returns>
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
    /// <summary>Carter endpoint that maps <c>PUT /clients/{id}</c>. Requires authorization.</summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the update-client route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/clients/{id:guid}", [Authorize] async (
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
            })
            .WithTags($"{nameof(Client)}s")
            .WithSummary("Update a client")
            .WithDescription("Updates an existing client's details including first name, last name, phone number, and notes.");
        }
    }
}