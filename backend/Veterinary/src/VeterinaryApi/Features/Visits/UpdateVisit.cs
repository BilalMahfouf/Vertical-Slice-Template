using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Features.Visits;

/// <summary>Vertical slice for updating an existing visit's clinical and payment details.</summary>
public static class UpdateVisit
{
    /// <summary>HTTP request body DTO for the update-visit endpoint.</summary>
    public sealed record Request(
        VisitType VisitType,
        List<string>? Symptoms,
        List<string>? Diagnosis,
        List<string>? Treatment,
        string? FollowUpNotes,
        decimal PaymentAmount,
        PaymentStatus PaymentStatus);

    /// <summary>Response DTO with the updated visit identifier.</summary>
    /// <param name="Id">The unique identifier of the updated visit.</param>
    public sealed record Response(Guid Id);

    /// <summary>Command merging the route ID with the updated visit fields.</summary>
    public sealed record UpdateVisitCommand(
        Guid Id,
        VisitType VisitType,
        List<string>? Symptoms,
        List<string>? Diagnosis,
        List<string>? Treatment,
        string? FollowUpNotes,
        decimal PaymentAmount,
        PaymentStatus PaymentStatus) : ICommand<Response>;

    /// <summary>
    /// FluentValidation validator with 4 rules: non-empty ID, valid <c>VisitType</c>,
    /// valid <c>PaymentStatus</c>, and optional <c>FollowUpNotes</c> capped at 2 000 characters.
    /// </summary>
    public sealed class Validator : AbstractValidator<UpdateVisitCommand>
    {
        /// <summary>Configures all update-visit validation rules.</summary>
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.VisitType)
                .IsInEnum();

            RuleFor(x => x.PaymentStatus)
                .IsInEnum();

            RuleFor(x => x.FollowUpNotes)
                .MaximumLength(2000);
        }
    }

    /// <summary>Handles the <see cref="UpdateVisitCommand"/> by loading the visit and applying updates.</summary>
    public sealed class UpdateVisitCommandHandler
        : ICommandHandler<UpdateVisitCommand, Response>
    {
        private readonly IApplicationDbContext _db;
        private readonly IValidator<UpdateVisitCommand> _validator;

        /// <summary>Initializes the handler with database and validator services.</summary>
        public UpdateVisitCommandHandler(
            IApplicationDbContext db,
            IValidator<UpdateVisitCommand> validator)
        {
            _db = db;
            _validator = validator;
        }

        /// <summary>
        /// Validates the command, loads the visit, calls <c>Visit.UpdateDetails()</c>, and persists.
        /// </summary>
        /// <returns>A successful result with the visit ID, or <c>VisitErrors.VisitNotFound</c>.</returns>
        public async Task<Result<Response>> Handle(UpdateVisitCommand command, CancellationToken cancellationToken = default)
        {
            _validator.ValidateAndThrow(command);
            var visit = await _db.Visits
                .FirstOrDefaultAsync(v => v.Id == command.Id,
                cancellationToken);
            if (visit is null)
            {
                return Result<Response>.Failure(
                    VisitErrors.VisitNotFound(command.Id));
            }
            visit.UpdateDetails(
                command.VisitType,
                command.Symptoms,
                command.Diagnosis,
                command.Treatment,
                command.FollowUpNotes,
                command.PaymentAmount,
                command.PaymentStatus);
            _db.Visits.Update(visit);
            await _db.SaveChangesAsync(cancellationToken);
            return Result<Response>.Success(
                new Response(visit.Id));
        }
    }

    /// <summary>Carter endpoint that maps <c>PUT /visits/{id}</c>. Requires authorization.</summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the update-visit route.</summary>
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
                    request.FollowUpNotes,
                    request.PaymentAmount,
                    request.PaymentStatus);
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.Problem();
            })
            .WithTags($"{nameof(Visit)}s")
            .WithSummary("Update a visit")
            .WithDescription("Updates an existing visit's details including visit type, symptoms, diagnosis, treatment, and follow-up notes.");
        }
    }
}
