using Microsoft.AspNetCore.Authorization;
using VeterinaryApi.Common.Endpoints;

namespace VeterinaryApi.Features.Push;

/// <summary>
/// Vertical slice that exposes the VAPID public key to authenticated clients.
/// The browser uses this key when creating a <c>PushSubscription</c> via the Web Push API.
/// </summary>
public static class GetVapidPublicKey
{
    /// <summary>Response DTO containing the VAPID public key.</summary>
    /// <param name="PublicKey">The URL-safe Base64-encoded VAPID public key.</param>
    public sealed record Response(string PublicKey);

    /// <summary>
    /// Carter endpoint that maps <c>GET /push/vapid-public-key</c>.
    /// Requires a valid JWT bearer token. The key is read from the
    /// <c>WEBPUSH_VAPID_PUBLIC_KEY</c> environment variable.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        /// <summary>Registers the <c>GET /push/vapid-public-key</c> route.</summary>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("push/vapid-public-key", [Authorize] () =>
            {
                var publicKey = Environment.GetEnvironmentVariable("WEBPUSH_VAPID_PUBLIC_KEY");

                if (string.IsNullOrWhiteSpace(publicKey))
                {
                    return Results.Problem(
                        detail: "VAPID public key is not configured on the server.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }

                return Results.Ok(new Response(publicKey));
            })
            .WithTags("Push Notifications")
            .WithSummary("Get VAPID public key")
            .WithDescription("Returns the VAPID public key required by the browser to create a Web Push subscription. Requires authentication.")
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}
