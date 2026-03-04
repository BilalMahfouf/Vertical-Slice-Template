using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Notifications;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Push;

/// <summary>
/// Vertical slice for removing a Web Push subscription for the current user.
/// Called when the user explicitly opts out of push notifications inside the app.
/// </summary>
public static class UnregisterPushSubscription
{
    /// <summary>
    /// Command carrying the endpoint to remove.
    /// </summary>
    /// <param name="Endpoint">The push endpoint URL to unsubscribe.</param>
    public sealed record Command(string Endpoint) : ICommand;

    /// <summary>Validates the endpoint field.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        /// <summary>Registers validation rules.</summary>
        public Validator()
        {
            RuleFor(c => c.Endpoint).NotEmpty().MaximumLength(2048);
        }
    }

    /// <summary>Handles the <see cref="Command"/> by deleting the subscription record.</summary>
    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;
        private readonly IValidator<Command> _validator;

        /// <summary>Initializes the handler with required services.</summary>
        public Handler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant,
            IValidator<Command> validator)
        {
            _db = db;
            _currentTenant = currentTenant;
            _validator = validator;
        }

        /// <summary>
        /// Finds the subscription by endpoint within the current tenant scope and deletes it.
        /// Returns success even if the subscription did not exist (idempotent).
        /// </summary>
        public async Task<Result> Handle(
            Command command,
            CancellationToken cancellationToken = default)
        {
            _validator.ValidateAndThrow(command);

            var userId = _currentTenant.UserId!.Value;

            var subscription = await _db.NotificationPushSubscriptions
                .ForTenant(userId)
                .FirstOrDefaultAsync(e => e.Endpoint == command.Endpoint, cancellationToken);

            if (subscription is not null)
            {
                _db.NotificationPushSubscriptions.Remove(subscription);
                await _db.SaveChangesAsync(cancellationToken);
            }

            // Idempotent — succeeds even if no record was found.
            return Result.Success;
        }
    }

    /// <summary>
    /// Carter endpoint that maps <c>DELETE /push/subscriptions</c>.
    /// Requires a valid JWT bearer token. Returns <c>204 No Content</c>.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the <c>DELETE /push/subscriptions</c> route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("push/subscriptions", [Authorize] async (
               [FromBody] Command command,
                [FromServices] ICommandHandler<Command> handler,
                CancellationToken cancellationToken = default) =>
            {
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.NoContent() : result.Problem();
            })
            .WithTags("Push Notifications")
            .WithSummary("Unregister a push subscription")
            .WithDescription("Removes the specified Web Push subscription for the current user. Idempotent — succeeds even if the subscription does not exist.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}
