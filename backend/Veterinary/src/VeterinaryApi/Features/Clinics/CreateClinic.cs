
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clinics;

namespace VeterinaryApi.Features.Clinics;

/// <summary>
/// Vertical slice for creating a new clinic associated with the authenticated doctor.
/// </summary>
public static class CreateClinic
{
    /// <summary>
    /// Command carrying the clinic's initial properties.
    /// Implements <see cref="ICommand{TResponse}"/> where the response is <see cref="Response"/>.
    /// Note: parameter names are lowercase — consider updating to PascalCase for consistency.
    /// </summary>
    public record CreateClinicCommand(
         string name,
         string phone,
         string address,
         int staffCount) : ICommand<Response>;

    /// <summary>Response DTO containing the newly created clinic's identifier.</summary>
    /// <param name="clinicId">The unique identifier of the created clinic.</param>
    public record Response(Guid clinicId);

    /// <summary>
    /// Handles the <see cref="CreateClinicCommand"/> by creating a <see cref="Clinic"/> aggregate
    /// owned by the currently authenticated user and persisting it.
    /// </summary>
    public class CreateClinicCommandHandler : ICommandHandler<CreateClinicCommand, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentUser;

        /// <summary>Initializes the handler with database and tenant context services.</summary>
        public CreateClinicCommandHandler(IApplicationDbContext db, ICurrentTenant currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Creates a new <see cref="Clinic"/> via <c>Clinic.Create()</c>, persists it, and returns its ID.
        /// </summary>
        /// <param name="command">The create-clinic command with clinic details.</param>
        /// <param name="cancellationToken">Token for cooperative cancellation.</param>
        /// <returns>A successful result with the new clinic ID.</returns>
        public async Task<Result<Response>> Handle(
            CreateClinicCommand command,
            CancellationToken cancellationToken)
        {

            var clinic = Clinic.Create(
                _currentUser.UserId!.Value,
                command.name,
                command.phone,
                command.address,
                command.staffCount);
            _db.Clinics.Add(clinic);
            await _db.SaveChangesAsync(cancellationToken);
            return Result<Response>.Success(new Response(clinic.Id));
        }

        /// <summary>
        /// Carter endpoint that maps <c>POST /clinics</c>.
        /// Requires authorization. Returns <c>201 Created</c> with the clinic ID, or Problem Details.
        /// </summary>
        public class Endpoint : IEndpoint
        {
            /// <summary>Registers the create-clinic route.</summary>
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost("/clinics", [Authorize] async (
                    [FromBody] CreateClinicCommand command,
                    ICommandHandler<CreateClinicCommand, Response> handler,
                    ICurrentTenant currentUser,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.Handle(command, cancellationToken);
                    return result.IsSuccess ? Results.Created("/", result.Value)
                    : result.Problem();

                })
                .WithTags($"{nameof(Clinic)}s")
                .WithSummary("Create a new clinic")
                .WithDescription("Creates a new clinic associated with the current authenticated doctor. Requires clinic name, phone, address, and staff count.");
            }
        }
    }
}
