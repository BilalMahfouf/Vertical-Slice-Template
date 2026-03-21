namespace VeterinaryApi.Domain.Subscriptions;

public enum SubscriptionStatus
{
    Pending = 1,
    Trialing,
    Active,
    PastDue,
    Cancelled,
    Expired
}
