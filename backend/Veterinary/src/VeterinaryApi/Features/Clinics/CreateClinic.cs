
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

public static class CreateClinic
{
    public record CreateClinicCommand(
         string name,
         string phone,
         string address,
         int staffCount) : ICommand<Response>;
    public record Response(Guid clinicId);

    public class CreateClinicCommandHandler : ICommandHandler<CreateClinicCommand, Response>
    {
        private readonly IApplicationDbContext _db;
        public CreateClinicCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }
        private readonly ICurrentUser _currentUser;
        public async Task<Result<Response>> Handle(
            CreateClinicCommand command,
            CancellationToken cancellationToken)
        {

            var clinic = Clinic.Create(
                _currentUser.UserId,
                command.name,
                command.phone,
                command.address,
                command.staffCount);
            _db.Clinics.Add(clinic);
            await _db.SaveChangesAsync(cancellationToken);
            return Result<Response>.Success(new Response(clinic.Id));
        }

        public class Endpoint : IEndpoint
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost("/clinics",[Authorize] async (
                    [FromBody] CreateClinicCommand command,
                    ICommandHandler<CreateClinicCommand, Response> handler,
                    ICurrentUser currentUser,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.Handle(command, cancellationToken);
                    return result.IsSuccess ? Results.Created("/", result.Value)
                    : result.Problem();

                }).WithTags("clinics");
            }
        }
    }
}
