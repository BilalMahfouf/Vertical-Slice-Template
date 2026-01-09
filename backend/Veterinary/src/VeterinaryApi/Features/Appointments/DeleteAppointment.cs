using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Appointments;

namespace VeterinaryApi.Features.Appointments;

public static class DeleteAppointment
{
    public sealed record DeleteAppointmentCommand(
        Guid id) : ICommand;

    public sealed class DeleteAppointmentCommandHandler
        : ICommandHandler<DeleteAppointmentCommand>
    {
        private readonly IApplicationDbContext _db;

        public DeleteAppointmentCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result> Handle(
            DeleteAppointmentCommand command,
            CancellationToken cancellationToken = default)
        {
            var appointment = await _db.Appointments
                .FirstOrDefaultAsync(e=>e.Id==command.id);
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
    public sealed class Endpoint : IEndpoint
    {
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
                .WithTags("Appointments")
                .WithName("DeleteAppointment");
        }
    }
}
