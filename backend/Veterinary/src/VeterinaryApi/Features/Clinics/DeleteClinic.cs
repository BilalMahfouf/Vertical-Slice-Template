using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clinics;

namespace VeterinaryApi.Features.Clinics;

/// <summary>
/// Vertical slice for soft-deleting a clinic by its unique identifier.
/// </summary>
public static class DeleteClinic
{
    /// <summary>
    /// Command carrying the target clinic identifier.
    /// Implements the non-generic <see cref="ICommand"/> (no return value).
    /// </summary>
    /// <param name="Id">The unique identifier of the clinic to delete.</param>
    public record DeleteClinicCommand(Guid Id) : ICommand;

    /// <summary>Handles the <see cref="DeleteClinicCommand"/> by soft-deleting the clinic entity.</summary>
    public class DeleteClinicCommandHandler : ICommandHandler<DeleteClinicCommand>
    {
        private readonly IApplicationDbContext _db;

        /// <summary>Initializes the handler with the application database context.</summary>
        public DeleteClinicCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Loads the clinic by ID, calls <c>Clinic.Delete()</c> (soft-delete), and persists.
        /// </summary>
        /// <param name="command">The delete-clinic command.</param>
        /// <param name="cancellationToken">Token for cooperative cancellation.</param>
        /// <returns>A successful result, or <c>ClinicErrors.ClinicNotFound</c>.</returns>
        public async Task<Result> Handle(
            DeleteClinicCommand command,
            CancellationToken cancellationToken)
        {
            var clinic = await _db.Clinics
                .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

            if (clinic is null)
            {
                return Result.Failure(ClinicErrors.ClinicNotFound(command.Id));
            }
            clinic.Delete();
            _db.Clinics.Update(clinic);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
    /// <summary>
    /// Carter endpoint that maps <c>DELETE /clinics/{id}</c>.
    /// Requires authorization. Returns <c>204 No Content</c> on success or Problem Details.
    /// </summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the delete-clinic route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/clinics/{id:guid}", [Authorize] async (
                Guid id,
                ICommandHandler<DeleteClinicCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteClinicCommand(id);
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.NoContent() :
                    result.Problem();
            })
            .WithTags($"{nameof(Clinic)}s")
            .WithSummary("Delete a clinic")
            .WithDescription("Soft deletes a clinic record by its unique identifier.");
        }
    }
}
