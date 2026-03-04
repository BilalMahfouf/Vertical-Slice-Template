using Microsoft.AspNetCore.Authorization;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Domain.Notifications;

namespace VeterinaryApi.Features.Push;

/// <summary>
/// Sends a test Web Push notification to all subscriptions registered for the current user.
/// Useful for verifying end-to-end push delivery without triggering a real domain event.
/// </summary>
public static class SendTestPush
{
    /// <summary>
    /// Carter endpoint that maps <c>POST /push/test</c>.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the <c>POST /push/test</c> route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("push/test", [Authorize] async (
                ICurrentTenant currentTenant,
                INotificatioService notificationService,
                CancellationToken cancellationToken) =>
            {
                var userId = currentTenant.UserId;
                if (userId is null)
                    return Results.Unauthorized();

                var testNotification = new NotificationResponse(
                    Id: Guid.NewGuid(),
                    Title: "🔔 VetiCloud Test",
                    Body: "Push notifications are working correctly!",
                    IsRead: false,
                    CreatedOnUtc: DateTime.UtcNow);

                // SendNotificationAsync sends both SignalR + Web Push.
                // We call it directly — no DB record is created for this test payload.
                await notificationService.SendNotificationAsync(
                    testNotification,
                    userId.Value,
                    cancellationToken);

                return Results.Ok(new { message = "Test push sent." });
            })
            .WithTags("Push Notifications")
            .WithSummary("Send test push notification")
            .WithDescription("Sends a test Web Push notification to all subscriptions registered for the authenticated user. Does not create a persistent notification record.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}
