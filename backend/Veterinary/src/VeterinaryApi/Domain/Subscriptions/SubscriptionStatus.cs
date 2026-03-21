namespace VeterinaryApi.Domain.Subscriptions;

public enum SubscriptionStatus
{
    Pending = 1,
    Trialing,
    Active,
    PaymentFailed,
    PastDue,
    Cancelled,
    Expired,
}
