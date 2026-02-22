using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Appointments;

namespace VeterinaryApi.Features.Appointments;

/// <summary>
/// Vertical slice for soft-deleting an appointment by its unique identifier.
/// Marks the appointment as deleted via the base <see cref="Domain.Common.Entity.Delete"/> method
/// without physically removing the row from the database.
/// </summary>
public static class DeleteAppointment
{
    /// <summary>Command carrying the appointment ID to delete.</summary>
    public sealed record DeleteAppointmentCommand(Guid id) : ICommand;

    /// <summary>
    /// Handles the <see cref="DeleteAppointmentCommand"/> by loading the appointment,
    /// calling the soft-delete method, and persisting the change.
    /// </summary>
    public sealed class DeleteAppointmentCommandHandler
        : ICommandHandler<DeleteAppointmentCommand>
    {
        private readonly IApplicationDbContext _db;

        /// <summary>Initializes the handler with the database context.</summary>
        public DeleteAppointmentCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Loads the appointment, applies a soft delete, and persists the change.
        /// </summary>
        /// <param name="command">The delete command with the target appointment ID.</param>
        /// <param name="cancellationToken">Token for cooperative cancellation.</param>
        /// <returns><see cref="Result.Success"/> or a failure result if the appointment is not found.</returns>
        public async Task<Result> Handle(
            DeleteAppointmentCommand command,
            CancellationToken cancellationToken = default)
        {
            var appointment = await _db.Appointments
                .FirstOrDefaultAsync(e => e.Id == command.id);
            if (appointment is null)
            {
                return Result.Failure(
                    AppointmentErrors.NotFound(command.id));
            }
            appointment.Delete();
            _db.Appointments.Update(appointment);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }

    /// <summary>
    /// Carter endpoint that maps <c>DELETE /appointments/{id}</c>.
    /// Returns <c>204 No Content</c> on success, or a Problem Details error response on failure.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the delete appointment route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/appointments/{id:guid}",
               [Authorize] async (Guid id,
                ICommandHandler<DeleteAppointmentCommand> handler,
                CancellationToken cancellationToken = default) =>
                {
                    var command = new DeleteAppointmentCommand(id);
                    var result = await handler.Handle(command, cancellationToken);
                    return result.IsSuccess ? Results.NoContent()
                    : result.Problem();
                })
                .WithTags($"{nameof(Appointment)}s")
                .WithSummary("Delete an appointment")
                .WithDescription("Soft deletes an appointment record by its unique identifier.")
                .WithName("DeleteAppointment");
        }
    }
}
