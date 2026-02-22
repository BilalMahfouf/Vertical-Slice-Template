using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain;
using VeterinaryApi.Domain.Animals;

namespace VeterinaryApi.Features.Animals;

/// <summary>
/// Vertical slice for updating an existing animal's details.
/// </summary>
public static class UpdateAnimal
{
    /// <summary>HTTP request body DTO for the update-animal endpoint.</summary>
    public record Request(
        string Name,
        string Species,
        string? Breed,
        Gender Gender,
        DateTime? BirthDate,
        string? Color,
        string? MicrochipNumber,
        AnimalStatus status);

    /// <summary>
    /// Command carrying the updated animal values plus the target ID.
    /// Implements the non-generic <see cref="ICommand"/> (no return value).
    /// </summary>
    public record UpdateAnimalCommand(
        Guid Id,
        string Name,
        string Species,
        string? Breed,
        Gender Gender,
        DateTime? BirthDate,
        string? Color,
        string? MicrochipNumber,
        AnimalStatus status) : ICommand;

    /// <summary>Handles the <see cref="UpdateAnimalCommand"/> by loading the animal and applying updates.</summary>
    public class UpdateAnimalCommandHandler : ICommandHandler<UpdateAnimalCommand>
    {
        private readonly IApplicationDbContext _db;

        /// <summary>Initializes the handler with the application database context.</summary>
        public UpdateAnimalCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Loads the animal, calls <c>Animal.UpdateDetails()</c>, and persists.
        /// </summary>
        /// <returns>A successful result, or <c>AnimalErrors.AnimalNotFound</c>.</returns>
        public async Task<Result> Handle(
            UpdateAnimalCommand command,
            CancellationToken cancellationToken)
        {
            var animal = await _db.Animals
                .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

            if (animal is null)
            {
                return Result.Failure(AnimalErrors.AnimalNotFound(command.Id));
            }

            animal.UpdateDetails(
                command.Name,
                command.Species,
                command.Breed ?? string.Empty,
                command.Gender,
                command.BirthDate,
                command.Color,
                command.status,
                command.MicrochipNumber);

            _db.Animals.Update(animal);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
    /// <summary>Carter endpoint that maps <c>PUT /animals/{id}</c>. Requires authorization.</summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the update-animal route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/animals/{id:guid}", [Authorize] async (
                Guid id,
                [FromBody] Request request,
                ICommandHandler<UpdateAnimalCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateAnimalCommand(
                    id,
                    request.Name,
                    request.Species,
                    request.Breed,
                    request.Gender,
                    request.BirthDate,
                    request.Color,
                    request.MicrochipNumber,
                    request.status);
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.NoContent() :
                    result.Problem();
            })
            .WithTags($"{nameof(Animal)}s")
            .WithSummary("Update an animal")
            .WithDescription("Updates an existing animal's details including name, species, breed, gender, birth date, color, microchip number, and status.");
        }
    }
}
