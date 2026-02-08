using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace VeterinaryApi.Infrastructure.Notifications;

[Authorize]
public class NotificationHub : Hub
{
}
