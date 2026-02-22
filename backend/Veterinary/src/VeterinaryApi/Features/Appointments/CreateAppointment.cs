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
using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Appointments;

/// <summary>
/// Vertical slice for creating a new appointment.
/// Encapsulates the command, response DTO, validation, command handler, and HTTP endpoint
/// in a single static class following the Vertical Slice Architecture pattern.
/// </summary>
public static class CreateAppointment
{
    /// <summary>
    /// Command carrying the data required to book a new appointment.
    /// The clinic is resolved automatically from the currently-authenticated doctor's clinic association.
    /// </summary>
    /// <param name="animalId">The animal being booked for the appointment.</param>
    /// <param name="AppointmentDate">The requested appointment date and time (must be in the future).</param>
    /// <param name="location">Optional location override (e.g., on-site visit address).</param>
    /// <param name="notes">Optional free-text notes about the appointment reason.</param>
    public sealed record CreateAppointmentCommand(
        Guid animalId,
        DateTime AppointmentDate,
        string? location,
        string? notes) : ICommand<Response>;

    /// <summary>Response DTO returned upon successful appointment creation.</summary>
    /// <param name="Id">The newly generated appointment identifier.</param>
    public sealed record Response(Guid Id);

    /// <summary>
    /// FluentValidation validator for <see cref="CreateAppointmentCommand"/>.
    /// Enforces that the animal ID is non-empty, the appointment date is in the future, and the location is provided.
    /// </summary>
    public sealed class Validator : AbstractValidator<CreateAppointmentCommand>
    {
        /// <summary>Registers validation rules.</summary>
        public Validator()
        {
            RuleFor(e => e.animalId).NotEmpty();

            RuleFor(e => e.AppointmentDate).NotEmpty()
                .GreaterThan(DateTime.UtcNow);

            RuleFor(e => e.location).NotEmpty();
        }
    }

    /// <summary>
    /// Handles the <see cref="CreateAppointmentCommand"/> by resolving the doctor's clinic,
    /// verifying the animal exists, creating the <see cref="Appointment"/> aggregate, and persisting it.
    /// </summary>
    public sealed class CreateAppointmentsCommandHander
        : ICommandHandler<CreateAppointmentCommand, Response>
    {
        private readonly IValidator<CreateAppointmentCommand> _validator;
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentUser;

        /// <summary>
        /// Initializes the handler with database context, validator, and current-user accessor.
        /// </summary>
        public CreateAppointmentsCommandHander(
            IApplicationDbContext db,
            IValidator<CreateAppointmentCommand> validator,
            ICurrentTenant currentUser)
        {
            _db = db;
            _validator = validator;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Executes the appointment creation workflow:
        /// <list type="number">
        ///   <item>Validates the command with FluentValidation (throws on failure).</item>
        ///   <item>Resolves the clinic owned by the currently authenticated doctor.</item>
        ///   <item>Verifies the specified animal exists.</item>
        ///   <item>Creates the <see cref="Appointment"/> aggregate and persists it.</item>
        /// </list>
        /// </summary>
        /// <param name="command">The incoming create-appointment command.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        /// <returns>
        /// A successful <see cref="Result{T}"/> containing the new appointment's ID,
        /// or a failure result with a <c>ClinicErrors.ClinicsNotFound</c> or
        /// <c>AnimalErrors.AnimalNotFound</c> error.
        /// </returns>
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
                return Result<Response>.Failure(ClinicErrors.ClinicsNotFound);
            }

            var isAnimalExist = await _db.Animals.AsNoTracking()
                .AnyAsync(e => e.Id == command.animalId);

            if (!isAnimalExist)
            {
                return Result<Response>.Failure(AnimalErrors.AnimalNotFound(command.animalId));
            }

            var appointment = Appointment.Create(
                command.animalId,
                clinicId,
                command.AppointmentDate,
                command.location,
                command.notes);

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();
            return Result<Response>.Success(new Response(appointment.Id));
        }
    }

    /// <summary>
    /// Carter endpoint that maps <c>POST /appointments</c> to the appointment creation handler.
    /// Requires an authenticated JWT bearer token (<c>[Authorize]</c>).
    /// Returns <c>201 Created</c> with the new appointment ID on success, or a Problem Details response on failure.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the <c>POST /appointments</c> route.</summary>
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
                        id = result.Value.Id,
                    })
                : result.Problem();
            })
            .WithTags($"{nameof(Appointment)}s")
            .WithSummary("Create a new appointment")
            .WithDescription("Creates a new appointment for an animal. Requires animal ID, appointment date, and location. The appointment date must be in the future.");
        }
    }
}
