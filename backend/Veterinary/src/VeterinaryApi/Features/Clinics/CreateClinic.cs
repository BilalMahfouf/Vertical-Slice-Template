
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
    public record Request(string name, string phone, string address);
    public record CreateClinicCommand(
        Guid doctorId,
        string name,
        string phone,
        string address) : ICommand<Response>;
    public record Response(Guid clinicId);

    public class CreateClinicCommandHandler : ICommandHandler<CreateClinicCommand, Response>
    {
        private readonly IApplicationDbContext _db;
        public CreateClinicCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Result<Response>> Handle(
            CreateClinicCommand command,
            CancellationToken cancellationToken)
        {
            var clinic = Clinic.Create(
                command.doctorId,
                command.name,
                command.phone,
                command.address);
            _db.Clinics.Add(clinic);
            await _db.SaveChangesAsync(cancellationToken);
            return Result<Response>.Success(new Response(clinic.Id));
        }

        public class Endpoint : IEndpoint
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost("/clinics", async (
                    [FromBody] Request request,
                    ICommandHandler<CreateClinicCommand, Response> handler,
                    ICurrentUser currentUser,
                    CancellationToken cancellationToken) =>
                {
                    var doctorId = currentUser.UserId;
                    var command = new CreateClinicCommand(
                        doctorId, // In a real application, retrieve the doctorId from the authenticated user context
                        name: request.name,
                        phone: request.phone,
                        address: request.address);
                    var result = await handler.Handle(command, cancellationToken);
                    return result.IsSuccess ? Results.Created("/",result.Value)
                    : result.Problem();
                    
                });
            }
        }
    }
}
