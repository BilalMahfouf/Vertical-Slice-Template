using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Notifications;

public static class NotificationErrors
{
    public static Error NotFound
        => Error.NotFound($"{nameof(Notification)}s.{nameof(NotFound)}",
            "Notifcations are not found or emtpy");
}
