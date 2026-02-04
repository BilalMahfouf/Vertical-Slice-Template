using Microsoft.AspNetCore.SignalR;
using VeterinaryApi.Domain.Notifications;

namespace VeterinaryApi.Infrastructure.Notifications;

public class NotificationService : INotificatioService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task SendNotificationAsync(Notification notification)
    {
return Task.CompletedTask;
    }
}
