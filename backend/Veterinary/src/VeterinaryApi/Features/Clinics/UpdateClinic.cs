using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clinics;

namespace VeterinaryApi.Features.Clinics;

public static class UpdateClinic
{
    public record Request(string Name, string Phone,string Address);
    public record UpdateClinicCommand(Guid Id, string Name,string Phone, string Address)
        : ICommand;
    public class UpdateClinicCommandHandler : ICommandHandler<UpdateClinicCommand>
    {
        private readonly IApplicationDbContext _db;
        public UpdateClinicCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }
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
            clinic.UpdateDetails(command.Name,command.Phone, command.Address);

            _db.Clinics.Update(clinic);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/clinics/{id:guid}",[Authorize] async (
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
            });
        }
    }
}
