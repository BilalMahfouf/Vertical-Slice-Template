namespace VeterinaryApi.Domain.Notifications;

public interface INotificatioService
{
    Task SendNotificationAsync(
        NotificationResponse notification,
        Guid UserId,
        CancellationToken cancellationToken = default);
}
public sealed record NotificationResponse(
    Guid Id,
    string Title,
    string Body,
    bool IsRead,
    DateTime CreatedOnUtc);
