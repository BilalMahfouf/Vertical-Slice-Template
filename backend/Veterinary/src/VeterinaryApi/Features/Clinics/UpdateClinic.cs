using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clinics;

namespace VeterinaryApi.Features.Clinics;

/// <summary>
/// Vertical slice for updating an existing clinic's details.
/// </summary>
public static class UpdateClinic
{
    /// <summary>Request body DTO for the update-clinic HTTP endpoint.</summary>
    /// <param name="Name">New clinic name.</param>
    /// <param name="Phone">New phone number.</param>
    /// <param name="Address">New address.</param>
    public record Request(string Name, string Phone, string Address);

    /// <summary>
    /// Command carrying updated clinic values together with the target ID.
    /// Implements the non-generic <see cref="ICommand"/> (no return value).
    /// </summary>
    public record UpdateClinicCommand(Guid Id, string Name, string Phone, string Address)
        : ICommand;

    /// <summary>Handles the <see cref="UpdateClinicCommand"/> by loading the clinic and applying updates.</summary>
    public class UpdateClinicCommandHandler : ICommandHandler<UpdateClinicCommand>
    {
        private readonly IApplicationDbContext _db;

        /// <summary>Initializes the handler with the application database context.</summary>
        public UpdateClinicCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Loads the clinic by ID, calls <c>Clinic.UpdateDetails()</c>, and persists.
        /// </summary>
        /// <param name="command">The update-clinic command.</param>
        /// <param name="cancellationToken">Token for cooperative cancellation.</param>
        /// <returns>A successful result, or <c>ClinicErrors.ClinicNotFound</c>.</returns>
        public async Task<Result> Handle(
            UpdateClinicCommand command,
            CancellationToken cancellationToken)
        {
            var clinic = await _db.Clinics
                .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);
            if (clinic is null)
            {
                return Result.Failure(ClinicErrors.ClinicNotFound(command.Id));
            }
            clinic.UpdateDetails(command.Name, command.Phone, command.Address);

            _db.Clinics.Update(clinic);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
    /// <summary>
    /// Carter endpoint that maps <c>PUT /clinics/{id}</c>.
    /// Requires authorization. Returns <c>204 No Content</c> on success or Problem Details.
    /// </summary>
    public class Endpoint : IEndpoint
    {
        /// <summary>Registers the update-clinic route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/clinics/{id:guid}", [Authorize] async (
                Guid id,
                [FromBody] Request request,
                ICommandHandler<UpdateClinicCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateClinicCommand(
                    id,
                    request.Name,
                    request.Phone,
                    request.Address);
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.NoContent() :
                    result.Problem();
            })
            .WithTags($"{nameof(Clinic)}s")
            .WithSummary("Update a clinic")
            .WithDescription("Updates an existing clinic's details including name, phone, and address.");
        }
    }
}
