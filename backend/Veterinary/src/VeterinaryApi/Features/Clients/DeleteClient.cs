using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clients;

namespace VeterinaryApi.Features.Clients;

public static class DeleteClient
{
    public record DeleteClientCommand(Guid Id) : ICommand;
    public class DeleteClientCommandHandler : ICommandHandler<DeleteClientCommand>
    {
        private readonly IApplicationDbContext _db;
        public DeleteClientCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }
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
    public class Endpoint : IEndpoint
    {
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