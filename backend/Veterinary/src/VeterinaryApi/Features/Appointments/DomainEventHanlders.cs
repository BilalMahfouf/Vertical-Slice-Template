using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Notifications;

namespace VeterinaryApi.Features.Appointments;

/// <summary>
/// Domain event handler that reacts to the <see cref="AppointmentCancelledDomainEvent"/>
/// and notifies the appointment's client via a persisted in-app notification delivered
/// over SignalR in real time.
/// </summary>
/// <remarks>
/// This handler is registered by Scrutor assembly scanning and is invoked by
/// <see cref="Infrastructure.CQRS.DomainEventPublisher"/> after the outbox message is dequeued
/// by <see cref="Infrastructure.OutboxMessages.ProcessOutboxMessagesJob"/>.
///
/// <b>Flow:</b>
/// <list type="number">
///   <item>Load the appointment details (client name, date) from the database.</item>
///   <item>Create a <see cref="Notification"/> aggregate with a cancellation message body.</item>
///   <item>Persist the notification (it will carry the event's <c>TenantId</c>).</item>
///   <item>Push the notification payload to the client's active SignalR connections.</item>
/// </list>
/// </remarks>
public sealed class AppointmentCancelledDomainEventHandler
    : IDomainEventHandler<AppointmentCancelledDomainEvent>
{
    private readonly IApplicationDbContext _db;
    private readonly INotificatioService _notificationService;

    /// <summary>
    /// Initializes the handler with database context and notification delivery service.
    /// </summary>
    /// <param name="db">The application database context for loading appointment data and persisting notifications.</param>
    /// <param name="notificationService">The SignalR notification delivery service.</param>
    public AppointmentCancelledDomainEventHandler(
        IApplicationDbContext db,
        INotificatioService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Handles the <see cref="AppointmentCancelledDomainEvent"/> by creating and delivering
    /// a cancellation notification to the appointment's owner.
    /// </summary>
    /// <param name="domainEvent">The cancellation event containing the appointment ID and tenant context.</param>
    /// <param name="cancellationToken">A token to observe for cooperative cancellation.</param>
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

        // Stamp the same tenant ID from the domain event to route the notification correctly.
        notification.TenantId = domainEvent.TenantId;

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);

        var notificationResponse = new NotificationResponse(
            notification.Id,
            notification.Title,
            notification.Body,
            notification.IsRead,
            notification.CreatedOnUtc);

        await _notificationService.SendNotificationAsync(
            notificationResponse,
            notification.TenantId,
            cancellationToken);
    }
}