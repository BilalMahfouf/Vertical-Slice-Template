namespace VeterinaryApi.Domain.Notifications;

public interface INotificatioService
{
    Task SendNotificationAsync(Notification notification);
}
