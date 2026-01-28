using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Clinics;

namespace VeterinaryApi.Features.Clinics;

public static class DeleteClinic
{
    public record DeleteClinicCommand(Guid Id) : ICommand;
    public class DeleteClinicCommandHandler : ICommandHandler<DeleteClinicCommand>
    {
        private readonly IApplicationDbContext _db;
        public DeleteClinicCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Result> Handle(
            DeleteClinicCommand command,
            CancellationToken cancellationToken)
        {
            var clinic = await _db.Clinics
                .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

            if (clinic is null)
            {
                return Result.Failure(ClinicErrors.ClinicNotFound(command.Id));
            }
            clinic.Delete();
            _db.Clinics.Update(clinic);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
    public class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/clinics/{id:guid}",[Authorize] async (
                Guid id,
                ICommandHandler<DeleteClinicCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteClinicCommand(id);
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.NoContent() :
                    result.Problem();
            });
        }
    }
}
