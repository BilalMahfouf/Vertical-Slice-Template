using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Animals;

namespace VeterinaryApi.Features.Animals;

/// <summary>
/// Vertical slice for soft-deleting an animal record.
/// </summary>
public static class DeleteAnimal
{
    /// <summary>
    /// Command carrying the target animal identifier.
    /// Implements the non-generic <see cref="ICommand"/> (no return value).
    /// </summary>
    /// <param name="Id">The unique identifier of the animal to soft-delete.</param>
    public record DeleteAnimalCommand(Guid Id) : ICommand;

    /// <summary>Handles the <see cref="DeleteAnimalCommand"/> by soft-deleting the animal entity.</summary>
    public class DeleteAnimalCommandHandler : ICommandHandler<DeleteAnimalCommand>
    {
        private readonly IApplicationDbContext _db;

        /// <summary>Initializes the handler with the application database context.</summary>
        public DeleteAnimalCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Loads the animal, calls <c>Animal.Delete()</c> (soft-delete), and persists.
        /// </summary>
        /// <param name="command">The delete-animal command.</param>
        /// <param name="cancellationToken">Token for cooperative cancellation.</param>
        /// <returns>A successful result, or <c>AnimalErrors.AnimalNotFound</c>.</returns>
        public async Task<Result> Handle(
            DeleteAnimalCommand command,
            CancellationToken cancellationToken)
        {
            var animal = await _db.Animals
                .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

            if (animal is null)
            {
                return Result.Failure(AnimalErrors.AnimalNotFound(command.Id));
            }
            animal.Delete();
            _db.Animals.Update(animal);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
    /// <summary>
    /// Carter endpoint that maps <c>DELETE /animals/{id}</c>.
    /// Requires authorization. Returns <c>204 No Content</c> on success or Problem Details.
    /// </summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the delete-animal route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/animals/{id:guid}", [Authorize] async (
                Guid id,
                ICommandHandler<DeleteAnimalCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteAnimalCommand(id);
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.NoContent() :
                    result.Problem();
            })
            .WithTags($"{nameof(Animal)}s")
            .WithSummary("Delete an animal")
            .WithDescription("Soft deletes an animal record by its unique identifier. The animal can be restored if needed.");
        }
    }
}