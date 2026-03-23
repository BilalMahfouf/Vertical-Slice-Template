using Microsoft.EntityFrameworkCore;
using Quartz;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Domain.Subscriptions;

namespace VeterinaryApi.Features.Subscriptions.BackgroundJobs;

internal sealed class MarkPastDueSubscriptionDailyJob(IApplicationDbContext db) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.UtcNow;
        var subscriptions = await db.Subscriptions
            .Where(e => e.Status == SubscriptionStatus.Active
            && e.CurrentPeriodEnd < now)
            .ToListAsync();
        if(subscriptions is null || subscriptions.Any() is false)
        {
            return;
        }
        foreach (var subscription in subscriptions)
        {
            subscription.MarkPastDue(); 
        }

        db.Subscriptions.UpdateRange(subscriptions);
        await db.SaveChangesAsync(context.CancellationToken);
    }
}
