using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain;
using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Domain.Clients;

namespace VeterinaryApi.Features.Animals;

public static class CreateAnimal
{
    public record Request(
        Guid ClientId,
        string Name,
        string Species,
        string? Breed,
        Gender Gender,
        DateTime? BirthDate,
        string? Color,
        string? MicrochipNumber);

    public record CreateAnimalCommand(
        Guid ClientId,
        string Name,
        string Species,
        string? Breed,
        Gender Gender,
        DateTime? BirthDate,
        string? Color,
        string? MicrochipNumber) : ICommand<Response>;

    public record Response(Guid Id);

    public class CreateAnimalCommandHandler : ICommandHandler<CreateAnimalCommand, Response>
    {
        private readonly IApplicationDbContext _db;

        public CreateAnimalCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result<Response>> Handle(
            CreateAnimalCommand command,
            CancellationToken cancellationToken)
        {
            var client = await _db.Clients
                .Where(c => c.Id == command.ClientId)
                .Select(c => new { c.Id, c.ClinicId })
                .FirstOrDefaultAsync(cancellationToken);

            if (client is null)
            {
                return Result<Response>.Failure(
                    ClientErrors.ClientNotFound(command.ClientId));
            }

            var animal = Animal.Create(
                client.ClinicId,
                command.ClientId,
                command.Name,
                command.Species,
                command.Breed,
                command.Gender,
                command.BirthDate,
                command.Color,
                command.MicrochipNumber);

            _db.Animals.Add(animal);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Response>.Success(new Response(animal.Id));
        }
    }

    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/animals", [Authorize]async (
                [FromBody] Request request,
                ICommandHandler<CreateAnimalCommand, Response> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateAnimalCommand(
                    request.ClientId,
                    request.Name,
                    request.Species,
                    request.Breed,
                    request.Gender,
                    request.BirthDate,
                    request.Color,
                    request.MicrochipNumber);

                var result = await handler.Handle(command, cancellationToken);

                return result.IsSuccess 
                    ? Results.Created($"/animals/{result.Value.Id}", result.Value)
                    : result.Problem();
            }).WithTags("animals");
        }
    }
}