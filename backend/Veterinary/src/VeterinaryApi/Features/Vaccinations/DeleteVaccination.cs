using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Vaccinations;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Vaccinations;

/// <summary>
/// Feature for deleting vaccination records.
/// </summary>
public static class DeleteVaccination
{
    /// <summary>
    /// Command to delete a vaccination record.
    /// </summary>
    /// <param name="Id">The unique identifier of the vaccination to delete.</param>
    public sealed record Command(Guid Id) : ICommand;
    /// <summary>
    /// Handler for deleting vaccination records.
    /// Retrieves and removes the vaccination from the database.
    /// </summary>
    public sealed class CommandHandler : ICommandHandler<Command>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;

        public CommandHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }
        public async Task<Result> Handle(
            Command command,
            CancellationToken cancellationToken = default)
        {
            var vaccination = await _db.Vaccinations
                .ForTenant(_currentTenant.UserId!.Value)
                .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);
            if (vaccination is null)
            {
                return Result.Failure(VaccinationErrors.NotFound);
            }
            _db.Vaccinations.Remove(vaccination);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
    /// <summary>
    /// Endpoint configuration for the delete vaccination route.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>
        /// Registers the DELETE /vaccinations/{id} endpoint.
        /// </summary>
        /// <remarks>
        /// Deletes a vaccination record by its ID.
        /// Requires authorization.
        /// Returns 200 OK on successful deletion or a problem response on failure.
        /// </remarks>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/vaccinations/{id:guid}", async (
                Guid id,
                ICommandHandler<Command> handler) =>
            {
                var command = new Command(id);
                var result = await handler.Handle(command);
                return result.IsSuccess ? Results.Ok()
                : result.Problem();
            })
            .WithTags($"{nameof(Vaccination)}s")
            .WithSummary("Delete a vaccination")
            .WithDescription("Deletes a vaccination record by its ID.")
            .RequireAuthorization();
        }
    }
}
