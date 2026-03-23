using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Diagnostics.CodeAnalysis;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Domain.Subscriptions;

namespace VeterinaryApi.Features.Subscriptions.BackgroundJobs;

internal sealed class MarkExpiredSubscriptionDailyJob(IApplicationDbContext db) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.UtcNow;
        var subscriptions = await db.Subscriptions
            .Where(e => e.Status == SubscriptionStatus.PastDue &&
            e.CurrentPeriodEnd.AddDays(1) < now)
            .ToListAsync(context.CancellationToken);
        if (subscriptions is null || !subscriptions.Any())
        {
            return;
        }
        foreach (var subscription in subscriptions)
        {
            subscription.MarkExpired();
        }

        db.Subscriptions.UpdateRange(subscriptions);
        await db.SaveChangesAsync(context.CancellationToken);

    }
}
