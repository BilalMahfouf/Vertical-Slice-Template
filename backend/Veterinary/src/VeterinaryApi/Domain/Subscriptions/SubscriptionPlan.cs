using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Subscriptions;

public sealed class SubscriptionPlan : Entity
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public string BillingInterval { get; private set; } = null!;  // "month" | "year"
    public int IntervalCount { get; private set; }
    public int TrialDays { get; private set; }
    public bool IsActive { get; private set; }

    private SubscriptionPlan() { } // EF Core

    public static SubscriptionPlan Create(
        string name,
        string slug,
        Money price,
        string billingInterval,
        int intervalCount = 1,
        int trialDays = 0)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Slug is required.");
        if (billingInterval is not "month" and not "year")
            throw new ArgumentException("Billing interval must be 'month' or 'year'.");

        return new SubscriptionPlan
        {
            Name            = name,
            Slug            = slug.ToLowerInvariant(),
            Price           = price,
            BillingInterval = billingInterval,
            IntervalCount   = intervalCount,
            TrialDays       = trialDays,
            IsActive        = true,
        };
    }

    public void Deactivate() => IsActive = false;
}
