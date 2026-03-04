using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Notifications;
using VeterinaryApi.Domain.Visits;
using VeterinaryApi.Infrastructure.OutboxMessages;

namespace VeterinaryApi.Infrastructure.Notifications.Jobs;

/// <summary>
/// A Quartz.NET background job that runs once daily at 08:00 AM and dispatches
/// reminder and motivational notifications to all active clinic owners.
/// </summary>
/// <remarks>
/// <b>What it does (four notification types):</b>
/// <list type="number">
///   <item>
///     <b>Appointment reminders</b> — confirmed or rescheduled appointments scheduled
///     within the next 24 hours (<see cref="UpcomingAppointmentReminderDomainEvent"/>).
///   </item>
///   <item>
///     <b>Vaccination reminders</b> — vaccinations whose next-due date falls within the
///     next 24 hours (<see cref="VaccinationDueDateReminderDomainEvent"/>).
///   </item>
///   <item>
///     <b>Pending-payment visit reminders</b> — visits whose payment status is still
///     <see cref="PaymentStatus.Pending"/> (<see cref="VisitPaymentPendingReminderDomainEvent"/>).
///   </item>
///   <item>
///     <b>Daily motivational message</b> — one per active user to encourage them to
///     record the day's clients (<see cref="MotivationalDailyReminderDomainEvent"/>).
///   </item>
/// </list>
///
/// <b>Implementation strategy:</b>
/// Instead of delivering notifications directly, this job serialises each domain event
/// into an <see cref="OutboxMessage"/> row (same format as
/// <c>InsertOutboxMessagesInterceptors</c>) and persists them in one
/// <c>SaveChangesAsync</c> call.  The existing <see cref="ProcessOutboxMessagesJob"/>
/// will pick them up within 10 seconds and invoke the appropriate
/// <c>IDomainEventHandler&lt;T&gt;</c>, guaranteeing reliable at-least-once delivery.
///
/// <b>Multi-tenancy:</b>
/// This job intentionally queries <em>all tenants</em> (no <c>.ForTenant()</c> filter)
/// because it must scan and notify every active clinic owner.  Each domain event is
/// stamped with the correct <c>TenantId</c> so downstream handlers route the
/// notification to the right user.
/// </remarks>
[DisallowConcurrentExecution]
public sealed class DailyRemindersJob : IJob
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    private readonly IApplicationDbContext _db;
    private readonly ILogger<DailyRemindersJob> _logger;

    public DailyRemindersJob(
        IApplicationDbContext db,
        ILogger<DailyRemindersJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Entry point called by the Quartz scheduler at 08:00 AM every day.
    /// Builds the full set of reminder <see cref="OutboxMessage"/> rows and
    /// persists them in a single database round-trip.
    /// </summary>
    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        var now = DateTime.UtcNow;
        var in24Hours = now.AddHours(24);

        _logger.LogInformation("DailyRemindersJob started at {Time}", now);

        var outboxMessages = new List<OutboxMessage>();

        // ── 1. Upcoming appointment reminders ────────────────────────────────
        var upcomingAppointments = await _db.Appointments
            .Where(a =>
                (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Rescheduled)
                && a.AppointmentDate >= now
                && a.AppointmentDate <= in24Hours)
            .Select(a => new { a.Id, a.TenantId })
            .ToListAsync(ct);

        foreach (var appt in upcomingAppointments)
        {
            var ev = new UpcomingAppointmentReminderDomainEvent(appt.Id)
            {
                TenantId = appt.TenantId
            };
            outboxMessages.Add(ToOutboxMessage(ev));
        }

        _logger.LogInformation("Queued {Count} appointment reminders", upcomingAppointments.Count);

        // ── 2. Vaccination due-date reminders ─────────────────────────────────
        var dueVaccinations = await _db.Vaccinations
            .Where(v => v.DueTo.HasValue && v.DueTo.Value >= now && v.DueTo.Value <= in24Hours)
            .Select(v => new { v.Id, v.TenantId })
            .ToListAsync(ct);

        foreach (var vacc in dueVaccinations)
        {
            var ev = new VaccinationDueDateReminderDomainEvent(vacc.Id)
            {
                TenantId = vacc.TenantId
            };
            outboxMessages.Add(ToOutboxMessage(ev));
        }

        _logger.LogInformation("Queued {Count} vaccination reminders", dueVaccinations.Count);

        // ── 3. Pending-payment visit reminders ────────────────────────────────
        var pendingVisits = await _db.Visits
            .Where(v => v.PaymentStatus == PaymentStatus.Pending)
            .Select(v => new { v.Id, v.TenantId })
            .ToListAsync(ct);

        foreach (var visit in pendingVisits)
        {
            var ev = new VisitPaymentPendingReminderDomainEvent(visit.Id)
            {
                TenantId = visit.TenantId
            };
            outboxMessages.Add(ToOutboxMessage(ev));
        }

        _logger.LogInformation("Queued {Count} pending-payment visit reminders", pendingVisits.Count);

        // ── 4. Motivational daily reminder (one per active user) ──────────────
        var userIds = await _db.Users
            .Select(u => u.Id)
            .ToListAsync(ct);

        foreach (var userId in userIds)
        {
            var ev = new MotivationalDailyReminderDomainEvent
            {
                TenantId = userId
            };
            outboxMessages.Add(ToOutboxMessage(ev));
        }

        _logger.LogInformation("Queued {Count} motivational reminders", userIds.Count);

        // ── Persist all outbox messages in one round-trip ─────────────────────
        if (outboxMessages.Count == 0)
        {
            _logger.LogInformation("DailyRemindersJob: nothing to queue today");
            return;
        }

        _db.OutboxMessages.AddRange(outboxMessages);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "DailyRemindersJob finished — {Total} outbox messages queued",
            outboxMessages.Count);
    }

    /// <summary>
    /// Serialises a domain event into an <see cref="OutboxMessage"/> using the same
    /// Newtonsoft.Json settings as <c>InsertOutboxMessagesInterceptors</c>.
    /// </summary>
    private static OutboxMessage ToOutboxMessage(Domain.Common.IDomainEvent @event) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = @event.GetType().AssemblyQualifiedName!,
            Content = JsonConvert.SerializeObject(@event, SerializerSettings),
            CreatedOnUtc = DateTime.UtcNow
        };
}
