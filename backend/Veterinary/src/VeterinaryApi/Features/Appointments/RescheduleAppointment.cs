using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Appointments;

namespace VeterinaryApi.Features.Appointments;

public static class RescheduleAppointment
{
    public sealed record Request(DateTime NewAppointmentDate);
    public sealed record RescheduleAppointmentCommand(
        Guid id,
        DateTime newAppointmentDate) : ICommand;

    public sealed class Validator : AbstractValidator<RescheduleAppointmentCommand>
    {
        public Validator()
        {
            RuleFor(e => e.id)
                .NotEmpty();
            RuleFor(e => e.newAppointmentDate)
                .NotEmpty()
                .GreaterThan(DateTime.UtcNow);
        }
    }
    public sealed class RescheduleAppointmentCommandHandler
        : ICommandHandler<RescheduleAppointmentCommand>
    {
        private readonly IValidator<RescheduleAppointmentCommand> _validator;
        private readonly IApplicationDbContext _db;
        public RescheduleAppointmentCommandHandler(
            IValidator<RescheduleAppointmentCommand> validator,
            IApplicationDbContext db)
        {
            _validator = validator;
            _db = db;
        }
        public async Task<Result> Handle(
            RescheduleAppointmentCommand command,
            CancellationToken cancellationToken = default)
        {
            _validator.ValidateAndThrow(command);
            var appointment = await _db.Appointments
                .FindAsync(command.id, cancellationToken);
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
    public sealed class Endpoint : IEndpoint
    {
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
                .WithTags("Appointments")
                .WithName("RescheduleAppointment");
        }
    }
}
