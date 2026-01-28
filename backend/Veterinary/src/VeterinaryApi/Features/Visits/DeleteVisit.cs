using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Features.Visits;

public static class DeleteVisit
{
    public sealed record DeleteVisitCommand(Guid Id) : ICommand;

    public sealed class DeleteVisitCommandHandler
                : ICommandHandler<DeleteVisitCommand>
    {
        private readonly IApplicationDbContext _db;
        public DeleteVisitCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }
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
    public sealed class Endpoint : IEndpoint
    {
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
                .WithTags("visits")
                .WithSummary("Deletes a visit by Id.")
                .WithDescription("Deletes a visit by Id.");
        }
    }
}
