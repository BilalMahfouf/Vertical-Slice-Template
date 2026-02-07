using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Common.Results;
using VeterinaryApi.Domain.Notifications;
using VeterinaryApi.Infrastructure.Persistence;

namespace VeterinaryApi.Features.Notifications;

public static class MarkNotifcationAsRead
{
    public sealed record Command(Guid NotificationId) : ICommand;

    public sealed class MarkNotificationAsReadCommandHandler : ICommandHandler<Command>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentTenant _currentTenant;
        public MarkNotificationAsReadCommandHandler(
            IApplicationDbContext db,
            ICurrentTenant currentTenant)
        {
            _db = db;
            _currentTenant = currentTenant;
        }
        public async Task<Result> Handle(
            Command command,
            CancellationToken cancellationToken = default)
        {
            var notification = await _db.Notifications
                .ForTenant(_currentTenant.UserId!.Value)
                .FirstOrDefaultAsync(
                e => e.Id == command.NotificationId
                , cancellationToken);
            if (notification is null)
            {
                return Result.Failure(NotificationErrors.NotFound);
            }
            notification.MarkAsRead();
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
    public sealed class Endpoint : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch("/notifications/{notificationId:guid}/mark-as-read", [Authorize] async (
                 Guid notificationId,
                 ICommandHandler<Command> handler,
                 CancellationToken ct) =>
            {
                var command = new Command(notificationId);
                var result = await handler.Handle(command, ct);
                return result.IsSuccess ? Results.NoContent() : result.Problem();
            })
            .WithTags($"{nameof(Notification)}s")
            .WithSummary("Mark notification as read")
            .WithDescription("Marks a specific notification as read by its unique identifier. The notification must belong to the current authenticated user.");
        }
    }
}
