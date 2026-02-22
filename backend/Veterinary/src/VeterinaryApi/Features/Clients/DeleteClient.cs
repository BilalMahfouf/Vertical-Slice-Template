using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clients;

namespace VeterinaryApi.Features.Clients;

/// <summary>
/// Vertical slice for soft-deleting a client record by its unique identifier.
/// </summary>
public static class DeleteClient
{
    /// <summary>Command carrying the target client identifier for soft-deletion.</summary>
    /// <param name="Id">The unique identifier of the client to soft-delete.</param>
    public record DeleteClientCommand(Guid Id) : ICommand;

    /// <summary>Handles the <see cref="DeleteClientCommand"/> by soft-deleting the client entity.</summary>
    public class DeleteClientCommandHandler : ICommandHandler<DeleteClientCommand>
    {
        private readonly IApplicationDbContext _db;

        /// <summary>Initializes the handler with the application database context.</summary>
        public DeleteClientCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Loads the client, calls <c>Client.Delete()</c>, and persists.</summary>
        /// <returns>A successful result, or <c>ClientErrors.ClientNotFound</c>.</returns>
        public async Task<Result> Handle(
            DeleteClientCommand command,
            CancellationToken cancellationToken)
        {
            var client = await _db.Clients
                .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

            if (client is null)
            {
                return Result.Failure(ClientErrors.ClientNotFound(command.Id));
            }
            client.Delete();
            _db.Clients.Update(client);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
    /// <summary>Carter endpoint that maps <c>DELETE /clients/{id}</c>. Requires authorization.</summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the delete-client route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/clients/{id:guid}", [Authorize] async (
                Guid id,
                ICommandHandler<DeleteClientCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteClientCommand(id);
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.NoContent() :
                    result.Problem();
            })
            .WithTags($"{nameof(Client)}s")
            .WithSummary("Delete a client")
            .WithDescription("Soft deletes a client record by its unique identifier. Associated animals will remain but orphaned.");
        }
    }
}