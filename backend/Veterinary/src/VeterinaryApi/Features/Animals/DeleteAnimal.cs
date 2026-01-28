using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Animals;

namespace VeterinaryApi.Features.Animals;

public static class DeleteAnimal
{
    public record DeleteAnimalCommand(Guid Id) : ICommand;
    public class DeleteAnimalCommandHandler : ICommandHandler<DeleteAnimalCommand>
    {
        private readonly IApplicationDbContext _db;
        public DeleteAnimalCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }
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
    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/animals/{id:guid}", [Authorize]async (
                Guid id,
                ICommandHandler<DeleteAnimalCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteAnimalCommand(id);
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.NoContent() :
                    result.Problem();
            });
        }
    }
}