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
/// Vertical slice for rescheduling an existing appointment to a new future date.
/// </summary>
public static class RescheduleAppointment
{
    /// <summary>HTTP request body containing the new appointment date.</summary>
    public sealed record Request(DateTime NewAppointmentDate);

    /// <summary>
    /// Command carrying the appointment ID and the new requested date/time.
    /// Implements <see cref="ICommand"/> (no return value beyond success/failure).
    /// </summary>
    public sealed record RescheduleAppointmentCommand(
        Guid id,
        DateTime newAppointmentDate) : ICommand;

    /// <summary>Validates that the appointment ID is non-empty and the new date is in the future.</summary>
    public sealed class Validator : AbstractValidator<RescheduleAppointmentCommand>
    {
        /// <summary>Registers validation rules.</summary>
        public Validator()
        {
            RuleFor(e => e.id)
                .NotEmpty();
            RuleFor(e => e.newAppointmentDate)
                .NotEmpty()
                .GreaterThan(DateTime.UtcNow);
        }
    }

    /// <summary>
    /// Handles the <see cref="RescheduleAppointmentCommand"/> by loading the appointment,
    /// calling <see cref="Appointment.Reschedule"/>, and saving the updated state.
    /// </summary>
    public sealed class RescheduleAppointmentCommandHandler
        : ICommandHandler<RescheduleAppointmentCommand>
    {
        private readonly IValidator<RescheduleAppointmentCommand> _validator;
        private readonly IApplicationDbContext _db;

        /// <summary>Initializes with a validator and database context.</summary>
        public RescheduleAppointmentCommandHandler(
            IValidator<RescheduleAppointmentCommand> validator,
            IApplicationDbContext db)
        {
            _validator = validator;
            _db = db;
        }

        /// <summary>
        /// Validates the command, loads the appointment, reschedules it, and persists the change.
        /// </summary>
        /// <param name="command">The reschedule command with appointment ID and new date.</param>
        /// <param name="cancellationToken">Token for cooperative cancellation.</param>
        /// <returns><see cref="Result.Success"/> or a failure result if the appointment is not found.</returns>
        public async Task<Result> Handle(
            RescheduleAppointmentCommand command,
            CancellationToken cancellationToken = default)
        {
            _validator.ValidateAndThrow(command);
            var appointment = await _db.Appointments
                .FirstOrDefaultAsync(e => e.Id == command.id, cancellationToken);
            if (appointment is null)
            {
                return Result.Failure(AppointmentErrors.NotFound(command.id));
            }
            appointment.Reschedule(command.newAppointmentDate);
            _db.Appointments.Update(appointment);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }

    /// <summary>
    /// Carter endpoint that maps <c>PUT /appointments/{id}/reschedule</c>.
    /// Returns <c>204 No Content</c> on success, or a Problem Details response on failure.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the reschedule route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/appointments/{id:guid}/reschedule",
                    [Authorize] async (
                    Guid id,
                    Request request,
                    ICommandHandler<RescheduleAppointmentCommand> handler,
                    CancellationToken cancellationToken) =>
                {
                    var command = new RescheduleAppointmentCommand(
                        id,
                        request.NewAppointmentDate);
                    var result = await handler.Handle(command, cancellationToken);
                    return result.IsSuccess
                        ? Results.NoContent()
                        : result.Problem();
                })
                .WithTags($"{nameof(Appointment)}s")
                .WithSummary("Reschedule an appointment")
                .WithDescription("Reschedules an existing appointment to a new date. The new date must be in the future.")
                .WithName("RescheduleAppointment");
        }
    }
}
