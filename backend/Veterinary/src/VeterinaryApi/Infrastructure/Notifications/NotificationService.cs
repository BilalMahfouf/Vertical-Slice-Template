using Microsoft.AspNetCore.SignalR;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Domain.Notifications;

namespace VeterinaryApi.Infrastructure.Notifications;

public class NotificationService : INotificatioService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ICurrentTenant _currentUser;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        ICurrentTenant currentUser)
    {
        _hubContext = hubContext;
        _currentUser = currentUser;
    }

    public async Task SendNotificationAsync(
        NotificationResponse notification,
        CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
                .User(_currentUser.UserId.ToString())
                .SendAsync("ReciveNotification", new { notification });
    }
}
