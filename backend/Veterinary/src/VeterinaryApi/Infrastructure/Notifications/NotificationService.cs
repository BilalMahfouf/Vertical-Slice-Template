using Microsoft.AspNetCore.SignalR;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Domain.Notifications;

namespace VeterinaryApi.Infrastructure.Notifications;

/// <summary>
/// Concrete implementation of <see cref="INotificatioService"/> that delivers real-time
/// push notifications to connected browser clients via SignalR.
/// </summary>
/// <remarks>
/// Notifications are routed to a specific user by their <see cref="Guid"/> identifier using
/// <c>IHubContext&lt;NotificationHub&gt;.Clients.User(userId)</c>. The SignalR framework maps
/// this to all active connections for that user based on the <c>NameIdentifier</c> claim.
///
/// The method sends the <c>"ReceiveNotification"</c> event to the targeted client(s). Frontend
/// subscribers must register a handler for this event name on their SignalR connection.
///
/// <b>Typo note:</b> The interface name <c>INotificatioService</c> (missing final 'n') is a
/// known naming inconsistency. It should be <c>INotificationService</c> in a future refactor.
/// </remarks>
public class NotificationService : INotificatioService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ICurrentTenant _currentUser;

    /// <summary>
    /// Initializes the service with the SignalR hub context and current-user accessor.
    /// </summary>
    /// <param name="hubContext">The SignalR hub context for broadcasting messages to clients.</param>
    /// <param name="currentUser">The ambient current-user context (reserved for future use).</param>
    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        ICurrentTenant currentUser)
    {
        _hubContext = hubContext;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Sends a notification payload to all active SignalR connections belonging to the specified user.
    /// </summary>
    /// <param name="notification">The notification data transfer object to send to the client.</param>
    /// <param name="UserId">The target user's unique identifier used to resolve their SignalR connections.</param>
    /// <param name="cancellationToken">A token to observe for cooperative cancellation.</param>
    public async Task SendNotificationAsync(
        NotificationResponse notification,
        Guid UserId,
        CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
                .User(UserId.ToString())
                .SendAsync("ReceiveNotification", new { notification });
    }
}
