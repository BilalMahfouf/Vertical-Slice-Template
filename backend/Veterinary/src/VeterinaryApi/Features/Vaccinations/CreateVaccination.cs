using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Vaccinations;
using VeterinaryApi.Domain.Visits;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Vaccinations;

/// <summary>
/// Feature for creating vaccination records.
/// </summary>
public static class CreateVaccination
{
    /// <summary>
    /// Command to create a new vaccination record.
    /// </summary>
    /// <param name="AnimalId">The unique identifier of the animal being vaccinated. Optional if VisitId is provided.</param>
    /// <param name="VisitId">The unique identifier of the associated visit. Optional if AnimalId is provided.</param>
    /// <param name="Name">The name of the vaccination. Maximum length of 100 characters.</param>
    /// <param name="GivenAt">The date and time when the vaccination was administered. Must be less than or equal to current UTC time.</param>
    /// <param name="DueTo">The date and time when the next vaccination is due. Must be greater than or equal to GivenAt.</param>
    /// <param name="Notes">Optional notes or remarks about the vaccination.</param>
    public sealed record CreateVaccinationCommand(
        Guid? AnimalId,
        Guid? VisitId,
        string Name,
        DateTime GivenAt,
        DateTime? DueTo,
        string? Notes) : ICommand<Response>;

    /// <summary>
    /// Validator for CreateVaccinationCommand with business rule validation.
    /// </summary>
    public sealed class Validator : AbstractValidator<CreateVaccinationCommand>
    {
        public Validator()
        {
            RuleFor(x => x.AnimalId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.GivenAt).GreaterThanOrEqualTo(DateTime.UtcNow.AddHours(-1));
            RuleFor(x => x.DueTo).GreaterThanOrEqualTo(x => x.GivenAt);
        }
    }

    /// <summary>
    /// Response containing the ID of the newly created vaccination.
    /// </summary>
    /// <param name="Id">The unique identifier of the created vaccination.</param>
    public sealed record Response(Guid Id);
    /// <summary>
    /// Handler for creating vaccination records.
    /// Resolves the AnimalId from VisitId if needed and creates the vaccination record.
    /// </summary>
    public sealed class CreateVaccinationCommandHandler
        : ICommandHandler<CreateVaccinationCommand, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly IValidator<CreateVaccinationCommand> _validator;
        private readonly ICurrentTenant _currentTenant;

        public CreateVaccinationCommandHandler(
            IApplicationDbContext db,
            IValidator<CreateVaccinationCommand> validator,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _validator = validator;
            _currentTenant = currentTenant;
        }

        public async Task<Result<Response>> Handle(
            CreateVaccinationCommand command,
            CancellationToken cancellationToken = default)
        {
            Guid animalId = Guid.Empty;
            if (command.VisitId is not null)
            {
                var visit = await _db.Visits
                    .ForTenant(_currentTenant.UserId!.Value)
                    .Where(e => e.Id == command.VisitId)
                    .Select(e => new
                    {
                        AnimalId = e.AnimalId
                    })
                    .FirstOrDefaultAsync(cancellationToken);
                if (visit is null)
                {
                    return Result<Response>
                        .Failure(VisitErrors.VisitsNotFound);
                }
                animalId = visit.AnimalId;
            }
            else if (command.AnimalId is not null)
            {
                animalId = command.AnimalId.Value;
            }
            if (animalId == Guid.Empty)
            {
                throw new ArgumentNullException(
                    nameof(animalId)
                    , "AnimalId must be not null");
            }
            var vaccination = Vaccination.Create(
                animalId,
                command.VisitId,
                command.Name,
                command.GivenAt,
                command.DueTo,
                command.Notes
                );
            _db.Vaccinations.Add(vaccination);
            await _db.SaveChangesAsync(cancellationToken);
            var response = new Response(vaccination.Id);
            return Result<Response>.Success(response);
        }
    }
    /// <summary>
    /// Endpoint configuration for the create vaccination route.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>
        /// Registers the POST /Vaccination endpoint.
        /// </summary>
        /// <remarks>
        /// Creates a new vaccination record for an animal.
        /// Requires either AnimalId or VisitId to be provided.
        /// </remarks>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/vaccinations", async (
                CreateVaccinationCommand command,
                ICommandHandler<CreateVaccinationCommand, Response> handler,
                CancellationToken ct = default) =>
            {
                var result = await handler.Handle(command, ct);
                return result.IsSuccess ? Results.Ok(result.Value)
                : result.Problem();
            })
            .WithTags($"{nameof(Vaccination)}s")
            .WithSummary("Create a new vaccination")
            .WithDescription("Creates a new vaccination record for an animal. Requires either AnimalId or VisitId to be provided.")
            .RequireAuthorization();
        }
    }
}
