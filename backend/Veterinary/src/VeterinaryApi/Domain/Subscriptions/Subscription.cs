using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Subscriptions;

public sealed class Subscription : Entity
{
    public Guid DoctorId { get; private set; }
    public Guid PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime CurrentPeriodStart { get; private set; }
    public DateTime CurrentPeriodEnd { get; private set; }
    public DateTime? TrialEndsAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public SubscriptionPlan Plan { get; private set; } = null!;
    private readonly List<Payment> _payments = [];
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private Subscription() { }

    public static Subscription Create(Guid doctorId, SubscriptionPlan plan)
    {
        var now = DateTime.UtcNow;
        var hasTrial = plan.TrialDays > 0;

        return new Subscription
        {
            DoctorId           = doctorId,
            PlanId             = plan.Id,
            Plan               = plan,
            Status             = hasTrial ? SubscriptionStatus.Trialing : SubscriptionStatus.Active,
            CurrentPeriodStart = now,
            CurrentPeriodEnd   = AddInterval(now, plan.BillingInterval, plan.IntervalCount),
            TrialEndsAt        = hasTrial ? now.AddDays(plan.TrialDays) : null,
            CreatedAt          = now,
            UpdatedAt          = now
        };
    }

    public void Activate()
    {
        Status    = SubscriptionStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPastDue()
    {
        Status    = SubscriptionStatus.PastDue;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidOperationException("Subscription is already cancelled.");

        Status      = SubscriptionStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt   = DateTime.UtcNow;
    }

    public void RenewPeriod()
    {
        CurrentPeriodStart = CurrentPeriodEnd;
        CurrentPeriodEnd   = AddInterval(CurrentPeriodEnd, Plan.BillingInterval, Plan.IntervalCount);
        Status             = SubscriptionStatus.Active;
        UpdatedAt          = DateTime.UtcNow;
    }

    public bool IsAccessGranted() =>
        Status is SubscriptionStatus.Trialing
               or SubscriptionStatus.Active
               or SubscriptionStatus.PastDue;

    private static DateTime AddInterval(DateTime from, string interval, int count) => interval switch
    {
        "month" => from.AddMonths(count),
        "year"  => from.AddYears(count),
        _       => throw new ArgumentException($"Unknown interval: {interval}")
    };
}
