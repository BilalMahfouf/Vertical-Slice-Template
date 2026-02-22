using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Features.Visits;

/// <summary>Vertical slice for soft-deleting a visit record by its unique identifier.</summary>
public static class DeleteVisit
{
    /// <summary>Command carrying the target visit identifier for soft-deletion.</summary>
    /// <param name="Id">The unique identifier of the visit to soft-delete.</param>
    public sealed record DeleteVisitCommand(Guid Id) : ICommand;

    /// <summary>Handles the <see cref="DeleteVisitCommand"/> by soft-deleting the visit entity.</summary>
    public sealed class DeleteVisitCommandHandler
                : ICommandHandler<DeleteVisitCommand>
    {
        private readonly IApplicationDbContext _db;

        /// <summary>Initializes the handler with the application database context.</summary>
        public DeleteVisitCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Loads the visit, calls <c>Visit.Delete()</c>, and persists the change.</summary>
        /// <returns>A successful result, or <c>VisitErrors.VisitNotFound</c>.</returns>
        public async Task<Result> Handle(
            DeleteVisitCommand command,
            CancellationToken cancellationToken = default)
        {
            var visit = await _db.Visits
                .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);
            if (visit is null)
            {
                return Result.Failure(
                    VisitErrors.VisitNotFound(command.Id));
            }
            visit.Delete();
            _db.Visits.Update(visit);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
    /// <summary>Carter endpoint that maps <c>DELETE /visits/{id}</c>. Requires authorization.</summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the delete-visit route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/visits/{id:guid}",
            [Authorize] async (Guid id,
                ICommandHandler<DeleteVisitCommand> handler,
                CancellationToken cancellationToken) =>
                {
                    var command = new DeleteVisitCommand(id);
                    var result = await handler.Handle(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.NoContent()
                        : result.Problem();
                })
                .WithTags($"{nameof(Visit)}s")
                .WithSummary("Delete a visit")
                .WithDescription("Soft deletes a visit record by its unique identifier.");
        }
    }
}
