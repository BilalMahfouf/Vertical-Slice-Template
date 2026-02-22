using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Errors;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Features.Visits;

/// <summary>
/// Vertical slice for creating a new veterinary visit, supporting two creation paths:
/// appointment-based (provide <c>AppointmentId</c>) or direct (provide <c>AnimalId</c> + <c>ClientId</c>).
/// </summary>
public static class CreateVisit
{
    /// <summary>
    /// Command containing all fields required to create a visit.
    /// Either <c>AppointmentId</c> OR both <c>AnimalId</c> and <c>ClientId</c> must be provided.
    /// </summary>
    public sealed record CreateVisitCommand(
        Guid? AnimalId,
        Guid? ClientId,
        Guid? AppointmentId,
        VisitType VisitType,
        List<string>? Symptoms,
        List<string>? Diagnosis,
        List<string>? Treatment,
        PaymentStatus PaymentStatus,
        decimal PaymentAmount,
        string? Notes) : ICommand<Response>;

    /// <summary>Response DTO with the newly created visit identifier.</summary>
    /// <param name="Id">The unique identifier of the created visit.</param>
    public sealed record Response(Guid Id);

    /// <summary>FluentValidation validator ensuring <c>VisitType</c> is a valid enum value.</summary>
    public sealed class Validator : AbstractValidator<CreateVisitCommand>
    {
        /// <summary>Configures the <c>VisitType</c> enum validation rule.</summary>
        public Validator()
        {
            RuleFor(v => v.VisitType)
                .IsInEnum().WithMessage("VisitType is invalid.");
        }
    }

    /// <summary>
    /// Handles the <see cref="CreateVisitCommand"/> by choosing the appropriate
    /// creation path and persisting the new visit.
    /// </summary>
    public sealed class CreateVisitCommandHandler
        : ICommandHandler<CreateVisitCommand, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly IValidator<CreateVisitCommand> _validator;

        /// <summary>Initializes the handler with database and validator services.</summary>
        public CreateVisitCommandHandler(
            IApplicationDbContext db,
            IValidator<CreateVisitCommand> validator)
        {
            _db = db;
            _validator = validator;
        }

        /// <summary>
        /// Creates a <see cref="Visit"/> from an existing appointment, loads the appointment with
        /// its associated animal, marks the appointment as completed, and returns the new visit.
        /// </summary>
        /// <returns>The created <see cref="Visit"/>, or <c>null</c> if the appointment was not found.</returns>
        private async Task<Visit?> CreateWithAppointment(CreateVisitCommand command)
        {
            Guid appointmentId = (Guid)command.AppointmentId!;
            var appointment = await _db.Appointments
                .Include(e => e.Animal)
                .FirstOrDefaultAsync(e => e.Id == appointmentId);
            if (appointment is null)
            {
                return null;
            }
            var visit = Visit.Create(
                appointment.AnimalId,
                appointment.Animal.ClientId,
                appointmentId,
                command.VisitType,
                command.Symptoms,
                command.Diagnosis,
                command.Treatment,
                command.Notes,
                appointment.AppointmentDate,
                command.PaymentAmount,
                command.PaymentStatus);

            appointment.Complete();

            return visit;
        }
        /// <summary>
        /// Validates the command, then creates the visit via appointment or direct path.
        /// Returns <c>AppointmentErrors.NotFound</c> or <c>Error.NotFound/Validation</c> on failure.
        /// </summary>
        public async Task<Result<Response>> Handle(
            CreateVisitCommand command,
            CancellationToken cancellationToken)
        {
            Visit visit;
            _validator.ValidateAndThrow(command);

            if (command.AppointmentId is not null)
            {
                var visitCreatedWithAppointmentId =
                     await CreateWithAppointment(command);
                if (visitCreatedWithAppointmentId is null)
                {
                    return Result<Response>.Failure(
                        AppointmentErrors.NotFound(command.AppointmentId.Value));
                }
                visit = visitCreatedWithAppointmentId;
            }
            else
            {
                if (command.AnimalId is null || command.ClientId is null)
                {
                    return Result<Response>.Failure(
                        Error.Validation(
                            "MissingIdentifiers",
                            "AnimalId and ClientId must be provided when AppointmentId is not specified."));
                }

                var isExist = await _db.Animals
                                 .AnyAsync(a => a.Id == command.AnimalId
                                 && a.ClientId == command.ClientId,
                                 cancellationToken);
                if (!isExist)
                {
                    return Result<Response>.Failure(
                        Error.NotFound(
                            "AnimalNotFound",
                            "The specified animal does not exist for the given client."));
                }

                visit = Visit.Create(
                   command.AnimalId.Value,
                   command.ClientId.Value,
                   null,
                   command.VisitType,
                   command.Symptoms,
                   command.Diagnosis,
                   command.Treatment,
                   command.Notes,
                   null,
                   command.PaymentAmount,
                   command.PaymentStatus);
            }
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Response>.Success(new Response(visit.Id));
        }
    }
    /// <summary>Carter endpoint that maps <c>POST /visits</c>. Requires authorization.</summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the create-visit route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/visits", [Authorize] async (
                CreateVisitCommand command,
                ICommandHandler<CreateVisitCommand, Response> handler,
                CancellationToken cancellationToken = default) =>
            {
                var result = await handler.Handle(command,
                    cancellationToken);
                return result.IsSuccess ?
                Results.Created($"/visits/{result.Value.Id}", new
                {
                    id = result.Value.Id
                }) : result.Problem();
            })
            .WithTags($"{nameof(Visit)}s")
            .WithSummary("Create a new visit")
            .WithDescription("Creates a new visit record. Can be created from an existing appointment (provide AppointmentId) or directly for an animal (provide AnimalId and ClientId). Includes symptoms, diagnosis, treatment, and notes.");
        }
    }
}
