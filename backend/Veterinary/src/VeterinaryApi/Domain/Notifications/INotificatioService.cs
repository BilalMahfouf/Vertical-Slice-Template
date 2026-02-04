namespace VeterinaryApi.Domain.Notifications;

public interface INotificatioService
{
    Task SendNotificationAsync(
        NotificationResponse notification,
        CancellationToken cancellationToken = default);
}
public sealed record NotificationResponse(Guid Id, string Title, string Body);
