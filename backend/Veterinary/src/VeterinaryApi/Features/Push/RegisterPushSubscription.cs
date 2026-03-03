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
/// Vertical slice for registering (or refreshing) a Web Push subscription for the current user.
/// If an identical endpoint already exists for this tenant, the encryption keys are updated (upsert).
/// </summary>
public static class RegisterPushSubscription
{
    /// <summary>
    /// Command carrying the subscription data from the browser's <c>PushSubscription</c> object.
    /// </summary>
    /// <param name="Endpoint">The push endpoint URL provided by the browser's push service.</param>
    /// <param name="P256dh">The P-256 DH public key from <c>pushSubscription.getKey("p256dh")</c>.</param>
    /// <param name="Auth">The authentication secret from <c>pushSubscription.getKey("auth")</c>.</param>
    /// <param name="UserAgent">Optional User-Agent string identifying the browser/device.</param>
    public sealed record Command(
        string Endpoint,
        string P256dh,
        string Auth,
        string? UserAgent) : ICommand<Response>;

    /// <summary>Response DTO returned upon successful registration.</summary>
    /// <param name="Id">The ID of the created or updated push subscription record.</param>
    public sealed record Response(Guid Id);

    /// <summary>Validates the required push subscription fields.</summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        /// <summary>Registers validation rules.</summary>
        public Validator()
        {
            RuleFor(c => c.Endpoint).NotEmpty().MaximumLength(2048);
            RuleFor(c => c.P256dh).NotEmpty().MaximumLength(512);
            RuleFor(c => c.Auth).NotEmpty().MaximumLength(256);
        }
    }

    /// <summary>
    /// Handles the <see cref="Command"/> by upserting the push subscription for the current user.
    /// </summary>
    public sealed class Handler : ICommandHandler<Command, Response>
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
        /// Checks whether the endpoint already exists for this tenant.
        /// If it does, updates the encryption keys. Otherwise creates a new record.
        /// </summary>
        public async Task<Result<Response>> Handle(
            Command command,
            CancellationToken cancellationToken = default)
        {
            _validator.ValidateAndThrow(command);

            var userId = _currentTenant.UserId!.Value;

            // Upsert by endpoint — the same browser re-subscribes with new keys after expiry.
            var existing = await _db.NotificationPushSubscriptions
                .ForTenant(userId)
                .FirstOrDefaultAsync(e => e.Endpoint == command.Endpoint, cancellationToken);

            if (existing is not null)
            {
                existing.UpdateKeys(command.P256dh, command.Auth);
                await _db.SaveChangesAsync(cancellationToken);
                return Result<Response>.Success(new Response(existing.Id));
            }

            var subscription = NotificationPushSubscription.Create(
                command.Endpoint,
                command.P256dh,
                command.Auth,
                command.UserAgent);

            _db.NotificationPushSubscriptions.Add(subscription);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Response>.Success(new Response(subscription.Id));
        }
    }

    /// <summary>
    /// Carter endpoint that maps <c>POST /push/subscriptions</c>.
    /// Requires a valid JWT bearer token.
    /// Returns <c>200 OK</c> with the subscription ID on success.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the <c>POST /push/subscriptions</c> route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("push/subscriptions", [Authorize] async (
               [FromBody] Command command,
                [FromServices]ICommandHandler<Command, Response> handler,
                CancellationToken cancellationToken = default) =>
            {
                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess
                    ? Results.Ok(new { id = result.Value!.Id })
                    : result.Problem();
            })
            .WithTags("Push Notifications")
            .WithSummary("Register a push subscription")
            .WithDescription("Stores or refreshes the current user's Web Push subscription. Upserts by endpoint so a re-subscription from the same browser does not create duplicates.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}
