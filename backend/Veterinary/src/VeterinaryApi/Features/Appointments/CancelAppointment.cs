using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Appointments;

namespace VeterinaryApi.Features.Appointments;

public static  class CancelAppointment
{
    public sealed record Request(string? notes);
    public sealed record CancelAppointmentCommand(
        Guid id,
        string? notes) : ICommand;

    public sealed class Validator : AbstractValidator<CancelAppointmentCommand>
    {
        public Validator()
        {
            RuleFor(e => e.id)
                .NotEmpty();
        }
    }

    public sealed class CancelAppointmentCommandHandler
        : ICommandHandler<CancelAppointmentCommand>
    {
        private readonly IValidator<CancelAppointmentCommand> _validator;
        private readonly IApplicationDbContext _db;

        public CancelAppointmentCommandHandler(
            IValidator<CancelAppointmentCommand> validator,
            IApplicationDbContext db)
        {
            _validator = validator;
            _db = db;
        }

        public async Task<Result> Handle(CancelAppointmentCommand command, CancellationToken cancellationToken = default)
        {
            _validator.ValidateAndThrow(command);

            var appointment = await _db.Appointments
                .FindAsync(command.id, cancellationToken);
            if( appointment is null)
            {
                return Result.Failure(AppointmentErrors.NotFound(command.id));
            }
            appointment.Cancel(command.notes);
            
            _db.Appointments.Update(appointment);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
    public sealed class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch("/appointments/{id:guid}/cancel",
                [Authorize]async (Guid id,
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
                .WithTags("Appointments")
                .WithName("Cancel Appointment");
        }
    }
}
