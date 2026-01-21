using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Features.Visits;

public static class UpdateVisit
{
    public sealed record Request(
        VisitType VisitType,
        List<string>? Symptoms,
        List<string>? Diagnosis,
        List<string>? Treatment,
        string? FollowUpNotes);
    public sealed record Response(Guid Id);

    public sealed record UpdateVisitCommand(
        Guid Id,
        VisitType VisitType,
        List<string>? Symptoms,
        List<string>? Diagnosis,
        List<string>? Treatment,
        string? FollowUpNotes ) : ICommand<Response>;

    public sealed class Validator : AbstractValidator<UpdateVisitCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.VisitType)
                .IsInEnum();

            RuleFor(x => x.FollowUpNotes)
                .MaximumLength(2000);
        }
    }

    public sealed class UpdateVisitCommandHandler
        : ICommandHandler<UpdateVisitCommand, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly IValidator<UpdateVisitCommand> _validator;

        public UpdateVisitCommandHandler(
            IApplicationDbContext db, 
            IValidator<UpdateVisitCommand> validator)
        {
            _db = db;
            _validator = validator;
        }

        public async Task<Result<Response>> Handle(UpdateVisitCommand command, CancellationToken cancellationToken = default)
        {
            _validator.ValidateAndThrow(command);
            var visit = await _db.Visits
                .FirstOrDefaultAsync(v => v.Id == command.Id,
                cancellationToken);
            if(visit is null)
            {
                return Result<Response>.Failure(
                    VisitErrors.VisitNotFound(command.Id));
            }
            visit.UpdateDetails(
                command.VisitType,
                command.Symptoms,
                command.Diagnosis,
                command.Treatment,
                command.FollowUpNotes);
            _db.Visits.Update(visit);
            await _db.SaveChangesAsync(cancellationToken);
            return Result<Response>.Success(
                new Response(visit.Id));
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/visits/{id:guid}", [Authorize] async (
                Guid id,
                Request request,
                ICommandHandler<UpdateVisitCommand, Response> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateVisitCommand(
                    id,
                    request.VisitType,
                    request.Symptoms,
                    request.Diagnosis,
                    request.Treatment,
                    request.FollowUpNotes);
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.Problem();
            }).WithTags("visits");
        }
    }
}
