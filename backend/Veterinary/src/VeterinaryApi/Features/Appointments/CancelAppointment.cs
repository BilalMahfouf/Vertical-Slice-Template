using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Appointments;

namespace VeterinaryApi.Features.Appointments;

/// <summary>
/// Vertical slice for cancelling an existing appointment.
/// Validates the command, loads and transitions the appointment aggregate, and persists the change.
/// </summary>
public static class CancelAppointment
{
    /// <summary>HTTP request body — optional cancellation notes from the caller.</summary>
    public sealed record Request(string? notes);

    /// <summary>
    /// Command carrying the appointment ID and optional notes for the cancellation.
    /// Implements <see cref="ICommand"/> (no return value beyond success/failure).
    /// </summary>
    public sealed record CancelAppointmentCommand(
        Guid id,
        string? notes) : ICommand;

    /// <summary>Validates that the appointment ID is non-empty.</summary>
    public sealed class Validator : AbstractValidator<CancelAppointmentCommand>
    {
        /// <summary>Registers validation rules.</summary>
        public Validator()
        {
            RuleFor(e => e.id)
                .NotEmpty();
        }
    }

    /// <summary>
    /// Handles the <see cref="CancelAppointmentCommand"/> by loading the appointment,
    /// calling <see cref="Appointment.Cancel"/> (which raises <see cref="AppointmentCancelledDomainEvent"/>),
    /// and persisting the state change.
    /// </summary>
    public sealed class CancelAppointmentCommandHandler
        : ICommandHandler<CancelAppointmentCommand>
    {
        private readonly IValidator<CancelAppointmentCommand> _validator;
        private readonly IApplicationDbContext _db;

        /// <summary>Initializes with a validator and database context.</summary>
        public CancelAppointmentCommandHandler(
            IValidator<CancelAppointmentCommand> validator,
            IApplicationDbContext db)
        {
            _validator = validator;
            _db = db;
        }

        /// <summary>
        /// Validates the command, finds the appointment, cancels it, and saves changes.
        /// </summary>
        /// <param name="command">The cancel command with appointment ID and optional notes.</param>
        /// <param name="cancellationToken">Token for cooperative cancellation.</param>
        /// <returns><see cref="Result.Success"/> or a failure result if the appointment is not found.</returns>
        public async Task<Result> Handle(CancelAppointmentCommand command, CancellationToken cancellationToken = default)
        {
            _validator.ValidateAndThrow(command);

            var appointment = await _db.Appointments
                .FirstOrDefaultAsync(e => e.Id == command.id, cancellationToken);
            if (appointment is null)
            {
                return Result.Failure(AppointmentErrors.NotFound(command.id));
            }
            appointment.Cancel(command.notes);

            _db.Appointments.Update(appointment);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }

    /// <summary>
    /// Carter endpoint that maps <c>PATCH /appointments/{id}/cancel</c>.
    /// Returns <c>204 No Content</c> on success or a Problem Details response on failure.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the cancellation route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch("/appointments/{id:guid}/cancel",
                [Authorize] async (Guid id,
                    CancelAppointment.Request request,
                    ICommandHandler<CancelAppointmentCommand> handler,
                    CancellationToken cancellationToken) =>
                {
                    var command = new CancelAppointmentCommand(
                        id,
                        request.notes);
                    var result = await handler.Handle(command, cancellationToken);
                    return result.IsSuccess ? Results.NoContent()
                        : result.Problem();
                })
                .WithTags($"{nameof(Appointment)}s")
                .WithSummary("Cancel an appointment")
                .WithDescription("Cancels an existing appointment. Optional notes can be provided for the cancellation reason.")
                .WithName("CancelAppointment");
        }
    }
}
