using Carter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using VeterinaryApi.Common.Endpoints;
using VeterinaryApi.Infrastructure.Notifications.Jobs;

namespace VeterinaryApi.Features.Notifications;

/// <summary>
/// Minimal-API endpoint that manually triggers the <see cref="DailyRemindersJob"/>
/// for testing and on-demand execution without waiting for the 08:00 AM cron schedule.
/// </summary>
/// <remarks>
/// <b>For development / QA use only.</b>
/// The job runs exactly as it would on schedule — scanning all tenants, building outbox
/// messages, and persisting them so the existing <c>ProcessOutboxMessagesJob</c> can
/// deliver them within 10 seconds.
/// </remarks>
public sealed class TriggerDailyReminders : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("notifications/trigger-daily-reminders", [Authorize] async (
            [FromServices] ISchedulerFactory schedulerFactory,
            CancellationToken cancellationToken) =>
        {
            var scheduler = await schedulerFactory.GetScheduler(cancellationToken);

            var jobKey = new JobKey(nameof(DailyRemindersJob));

            // If the scheduler does not know this job at all, return 404.
            if (!await scheduler.CheckExists(jobKey, cancellationToken))
            {
                return Results.NotFound("DailyRemindersJob is not registered in the scheduler.");
            }

            await scheduler.TriggerJob(jobKey, cancellationToken);

            return Results.Accepted(value: new
            {
                message = "DailyRemindersJob triggered. Notifications will be delivered within ~10 seconds."
            });
        })
        .WithTags("Notifications")
        .WithSummary("[DEV] Trigger daily reminders job")
        .WithDescription(
            "Manually triggers the DailyRemindersJob for testing purposes. " +
            "The job scans all upcoming appointments, due vaccinations, pending-payment " +
            "visits, and sends a motivational reminder to every active user.");
    }
}
