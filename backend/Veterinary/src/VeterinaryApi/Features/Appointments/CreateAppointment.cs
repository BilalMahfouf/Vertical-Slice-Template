using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Mozilla;
using System.ComponentModel.DataAnnotations;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Appointments;

public static class CreateAppointment
{
    public sealed record CreateAppointmentCommand(
        Guid animalId,
        DateTime AppointmentDate,
        TimeSpan? appointmentTime,
        string? location,
        string? notes) : ICommand<Response>;

    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<CreateAppointmentCommand>
    {
        public Validator()
        {
            RuleFor(e => e.animalId).NotEmpty();

            RuleFor(e => e.AppointmentDate).NotEmpty()
                .GreaterThan(DateTime.UtcNow);

            RuleFor(e => e.appointmentTime).NotEmpty()
                .GreaterThan(TimeSpan.Zero);

            RuleFor(e => e.location).NotEmpty();
        }
    }
    public sealed class CreateAppointmentsCommandHander
        : ICommandHandler<CreateAppointmentCommand, Response>
    {
        private readonly IValidator<CreateAppointmentCommand> _validator;
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUser _currentUser;

        public CreateAppointmentsCommandHander(
            IApplicationDbContext db,
            IValidator<CreateAppointmentCommand> validator,
            ICurrentUser currentUser)
        {
            _db = db;
            _validator = validator;
            _currentUser = currentUser;
        }

        public async Task<Result<Response>> Handle(
            CreateAppointmentCommand command,
            CancellationToken cancellationToken = default)
        {
            _validator.ValidateAndThrow(command);
            var clinicId = await _db.Clinics.AsNoTracking()
                .Where(e => e.DoctorId == _currentUser.UserId)
                .Select(e => e.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (clinicId == Guid.Empty)
            {

                return Result<Response>.Failure(
                    ClinicErrors.ClinicsNotFound);
            }
            var appointment = Appointment.Create(
                command.animalId,
                clinicId,
                command.AppointmentDate,
                command.appointmentTime,
                command.location,
                command.notes);

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();
            return Result<Response>.Success(new Response(appointment.Id));
        }
    }
    public sealed class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("appointments", [Authorize] async (
                 CreateAppointmentCommand command,
                ICommandHandler<CreateAppointmentCommand, Response> handler,
                CancellationToken cancellationToken = default) =>
            {
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.Created(
                    $"/appointments/{result.Value.Id}", new
                    {
                        id=result.Value.Id,
                    })
                : result.Problem();
            }).WithTags("appointments");
        }
    }
}
