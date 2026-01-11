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

public static class UpdateAnimal
{
    public record Request(
        string Name,
        string Species,
        string? Breed,
        Gender Gender,
        DateTime? BirthDate,
        string? Color,
        string? MicrochipNumber,
        AnimalStatus status);

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

    public class UpdateAnimalCommandHandler : ICommandHandler<UpdateAnimalCommand>
    {
        private readonly IApplicationDbContext _db;
        public UpdateAnimalCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }
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
    public class Endpoint : IEndpoint
    {
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
            }).WithTags("animals");
        }
    }
}
