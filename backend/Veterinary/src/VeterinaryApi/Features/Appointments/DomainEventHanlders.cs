using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Notifications;

namespace VeterinaryApi.Features.Appointments;

public sealed class AppointmentCancelledDomainEventHandler
    : IDomainEventHandler<AppointmentCancelledDomainEvent>
{
    private readonly IApplicationDbContext _db;
    private readonly INotificatioService _notificationService;

    public AppointmentCancelledDomainEventHandler(
        IApplicationDbContext db,
        INotificatioService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task Handle(
        AppointmentCancelledDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var data = await _db.Appointments
            .Where(e => e.Id == domainEvent.AppointmentId)
            .Select(e => new
            {
                ClientName = e.Animal.Client.FullName,
                AppointmentDate = e.AppointmentDate,
                CreatedOnUtc = e.CreatedOnUtc,

            }).FirstOrDefaultAsync(cancellationToken);
        if (data is null)
        {
            return;
        }
        var body = $"Appointment on the date {data.AppointmentDate}," +
            $" for the client {data.ClientName} is cancelled";
        var notification = Notification.Create(
            "Appointment Cancelled",
            body);
        notification.TenantId = domainEvent.TenantId;
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);
        var notificationResponse = new NotificationResponse(
            notification.Id,
            notification.Title,
            notification.Body);
        await _notificationService.SendNotificationAsync(notificationResponse, cancellationToken);

    }
}