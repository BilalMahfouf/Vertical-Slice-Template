
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Vaccinations;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Vaccinations;

/// <summary>
/// Feature for updating vaccination records.
/// </summary>
public static class UpdateVaccination
{
    /// <summary>
    /// Request body for updating a vaccination record.
    /// </summary>
    /// <param name="Name">The name of the vaccination. Maximum length of 100 characters.</param>
    /// <param name="GivenAt">The date and time when the vaccination was administered. Must be less than or equal to current UTC time.</param>
    /// <param name="DueTo">The date and time when the next vaccination is due. Must be greater than or equal to GivenAt.</param>
    /// <param name="Notes">Optional notes or remarks about the vaccination.</param>
    public sealed record Request(
        string Name,
        DateTime GivenAt,
        DateTime? DueTo,
        string? Notes
        );

    /// <summary>
    /// Command to update a vaccination record.
    /// </summary>
    /// <param name="Id">The unique identifier of the vaccination to update.</param>
    /// <param name="Name">The name of the vaccination. Maximum length of 100 characters.</param>
    /// <param name="GivenAt">The date and time when the vaccination was administered. Must be less than or equal to current UTC time.</param>
    /// <param name="DueTo">The date and time when the next vaccination is due. Must be greater than or equal to GivenAt.</param>
    /// <param name="Notes">Optional notes or remarks about the vaccination.</param>
    public sealed record Command(
        Guid Id,
        string Name,
        DateTime GivenAt,
        DateTime? DueTo,
        string? Notes
        ) : ICommand;

    /// <summary>
    /// Validator for UpdateVaccination.Command with business rule validation.
    /// </summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.GivenAt).GreaterThanOrEqualTo(DateTime.UtcNow.AddDays(-1));
            RuleFor(x => x.DueTo).GreaterThanOrEqualTo(x => x.GivenAt);
        }
    }
    /// <summary>
    /// Handler for updating vaccination records.
    /// Validates the command and updates the vaccination information in the database.
    /// </summary>
    public sealed class UpdateVaccinationCommandHandler
        : ICommandHandler<Command>
    {
        private readonly IApplicationDbContext _db;
        private readonly IValidator<Command> _validator;
        private readonly ICurrentTenant _currentTenant;

        public UpdateVaccinationCommandHandler(
            IApplicationDbContext db,
            IValidator<Command> validator,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _validator = validator;
            _currentTenant = currentTenant;
        }
        public async Task<Result> Handle(
            Command command,
            CancellationToken cancellationToken = default)
        {
            _validator.ValidateAndThrow(command);
            var vaccination = await _db.Vaccinations
                .ForTenant(_currentTenant.UserId!.Value)
                .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);
            if (vaccination is null)
            {
                return Result.Failure(VaccinationErrors.NotFound);
            }
            vaccination.Update(
                command.Name,
                command.GivenAt,
                command.DueTo,
                command.Notes);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }

    /// <summary>
    /// Endpoint configuration for the update vaccination route.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>
        /// Registers the PUT /vaccinations/{id} endpoint.
        /// </summary>
        /// <remarks>
        /// Updates an existing vaccination record.
        /// Requires authorization.
        /// Returns 200 OK on successful update or a problem response on failure.
        /// </remarks>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/vaccinations/{id:guid}", async (
                Guid id,
                Request request,
                ICommandHandler<Command> handler) =>
            {
                var command = new Command(
                    id,
                    request.Name,
                    request.GivenAt,
                    request.DueTo,
                    request.Notes);

                var result = await handler.Handle(command);

                return result.IsSuccess ? Results.Ok()
                : result.Problem();
            })
            .WithTags($"{nameof(Vaccination)}s")
            .WithSummary("Update a vaccination")
            .WithDescription("Updates an existing vaccination record with new information.")
            .RequireAuthorization();
        }
    }
}
