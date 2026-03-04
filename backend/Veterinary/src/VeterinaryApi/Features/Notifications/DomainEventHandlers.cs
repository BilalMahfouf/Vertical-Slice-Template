using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.CQRS;
using VeterinaryApi.Domain.Notifications;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Features.Notifications;

/// <summary>
/// Handles <see cref="VisitPaymentPendingReminderDomainEvent"/> by creating a persisted
/// in-app notification and delivering it via SignalR and Web Push to the clinic owner.
/// </summary>
public sealed class VisitPaymentPendingReminderDomainEventHandler
    : IDomainEventHandler<VisitPaymentPendingReminderDomainEvent>
{
    private readonly IApplicationDbContext _db;
    private readonly INotificatioService _notificationService;

    public VisitPaymentPendingReminderDomainEventHandler(
        IApplicationDbContext db,
        INotificatioService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task Handle(
        VisitPaymentPendingReminderDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var data = await _db.Visits
            .Where(v => v.Id == domainEvent.VisitId)
            .Select(v => new
            {
                ClientName = v.Owner.FullName,
                AnimalName = v.Animal.Name,
                v.PaymentAmount
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
        {
            return;
        }

        var body = $"Le client {data.ClientName} a un paiement en attente de {data.PaymentAmount:F2} DA pour l'animal {data.AnimalName}.";

        var notification = Notification.Create("Rappel — Paiement en attente", body);
        notification.TenantId = domainEvent.TenantId;

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);

        var response = new NotificationResponse(
            notification.Id,
            notification.Title,
            notification.Body,
            notification.IsRead,
            notification.CreatedOnUtc);

        await _notificationService.SendNotificationAsync(response, notification.TenantId, cancellationToken);
    }
}

/// <summary>
/// Handles <see cref="VaccinationDueDateReminderDomainEvent"/> by creating a persisted
/// in-app notification and delivering it via SignalR and Web Push to the clinic owner.
/// </summary>
public sealed class VaccinationDueDateReminderDomainEventHandler
    : IDomainEventHandler<VaccinationDueDateReminderDomainEvent>
{
    private readonly IApplicationDbContext _db;
    private readonly INotificatioService _notificationService;

    public VaccinationDueDateReminderDomainEventHandler(
        IApplicationDbContext db,
        INotificatioService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task Handle(
        VaccinationDueDateReminderDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var data = await _db.Vaccinations
            .Where(v => v.Id == domainEvent.VaccinationId)
            .Select(v => new
            {
                ClientName = v.Animal.Client.FullName,
                AnimalName = v.Animal.Name,
                VaccineName = v.Name,
                v.DueTo
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
        {
            return;
        }

        var body = $"L'animal {data.AnimalName} du client {data.ClientName} est dû pour le vaccin '{data.VaccineName}' le {data.DueTo:dd/MM/yyyy}.";

        var notification = Notification.Create("Rappel — Vaccination à venir", body);
        notification.TenantId = domainEvent.TenantId;

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);

        var response = new NotificationResponse(
            notification.Id,
            notification.Title,
            notification.Body,
            notification.IsRead,
            notification.CreatedOnUtc);

        await _notificationService.SendNotificationAsync(response, notification.TenantId, cancellationToken);
    }
}

/// <summary>
/// Handles <see cref="UpcomingAppointmentReminderDomainEvent"/> by creating a persisted
/// in-app notification and delivering it via SignalR and Web Push to the clinic owner.
/// </summary>
public sealed class UpcomingAppointmentReminderDomainEventHandler
    : IDomainEventHandler<UpcomingAppointmentReminderDomainEvent>
{
    private readonly IApplicationDbContext _db;
    private readonly INotificatioService _notificationService;

    public UpcomingAppointmentReminderDomainEventHandler(
        IApplicationDbContext db,
        INotificatioService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task Handle(
        UpcomingAppointmentReminderDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var data = await _db.Appointments
            .Where(a => a.Id == domainEvent.AppointmentId)
            .Select(a => new
            {
                ClientName = a.Animal.Client.FullName,
                AnimalName = a.Animal.Name,
                a.AppointmentDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
        {
            return;
        }

        var body = $"Rendez-vous à venir pour l'animal {data.AnimalName} du client {data.ClientName} le {data.AppointmentDate:dd/MM/yyyy} à {data.AppointmentDate:HH:mm}.";

        var notification = Notification.Create("Rappel — Rendez-vous à venir", body);
        notification.TenantId = domainEvent.TenantId;

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);

        var response = new NotificationResponse(
            notification.Id,
            notification.Title,
            notification.Body,
            notification.IsRead,
            notification.CreatedOnUtc);

        await _notificationService.SendNotificationAsync(response, notification.TenantId, cancellationToken);
    }
}

/// <summary>
/// Handles <see cref="MotivationalDailyReminderDomainEvent"/> by creating a persisted
/// in-app notification with a randomly selected motivational message and delivering it
/// via SignalR and Web Push to the target clinic owner.
/// </summary>
public sealed class MotivationalDailyReminderDomainEventHandler
    : IDomainEventHandler<MotivationalDailyReminderDomainEvent>
{
    private static readonly string[] MotivationalMessages =
    [
        "C'est le moment d'enregistrer vos derniers clients sur le système.",
        "Bonne journée ! N'oubliez pas de saisir les visites d'aujourd'hui.",
        "Commencez la journée en mettant à jour les dossiers de vos patients.",
        "Chaque nouveau jour est une occasion de mieux prendre soin des animaux.",
        "Organisez vos fiches aujourd'hui pour rester toujours à jour."
    ];

    private readonly IApplicationDbContext _db;
    private readonly INotificatioService _notificationService;

    public MotivationalDailyReminderDomainEventHandler(
        IApplicationDbContext db,
        INotificatioService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task Handle(
        MotivationalDailyReminderDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var body = MotivationalMessages[Random.Shared.Next(MotivationalMessages.Length)];

        var notification = Notification.Create("Rappel du jour", body);
        notification.TenantId = domainEvent.TenantId;

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);

        var response = new NotificationResponse(
            notification.Id,
            notification.Title,
            notification.Body,
            notification.IsRead,
            notification.CreatedOnUtc);

        await _notificationService.SendNotificationAsync(response, notification.TenantId, cancellationToken);
    }
}
