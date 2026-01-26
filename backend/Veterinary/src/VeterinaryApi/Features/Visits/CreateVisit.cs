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

public static class CreateVisit
{
    public sealed record CreateVisitCommand(
        Guid? AnimalId,
        Guid? ClientId,
        Guid? AppointmentId,
        VisitType VisitType,
        List<string>? Symptoms,
        List<string>? Diagnosis,
        List<string>? Treatment,
        string? Notes) : ICommand<Response>;
    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<CreateVisitCommand>
    {
        public Validator()
        {
            RuleFor(v => v.VisitType)
                .IsInEnum().WithMessage("VisitType is invalid.");
        }
    }

    public sealed class CreateVisitCommandHandler
        : ICommandHandler<CreateVisitCommand, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly IValidator<CreateVisitCommand> _validator;
        public CreateVisitCommandHandler(
            IApplicationDbContext db,
            IValidator<CreateVisitCommand> validator)
        {
            _db = db;
            _validator = validator;
        }
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
            var visit= Visit.Create(
                appointment.AnimalId,
                appointment.Animal.ClientId,
                appointmentId,
                command.VisitType,
                command.Symptoms,
                command.Diagnosis,
                command.Treatment,
                command.Notes);

            appointment.Complete();
            
            return visit;
        }
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
                if(command.AnimalId is null || command.ClientId is null)
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
                   command.Notes);
            }
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Response>.Success(new Response(visit.Id));
        }
    }
    public sealed class Endpoint : IEndpoint
    {
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
            });
        }
    }
}
