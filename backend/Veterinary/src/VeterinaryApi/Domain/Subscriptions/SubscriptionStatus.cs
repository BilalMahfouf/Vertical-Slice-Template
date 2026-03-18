namespace VeterinaryApi.Domain.Subscriptions;

public enum SubscriptionStatus
{
    Trialing=1,
    Active,
    PastDue,
    Cancelled,
    Expired
}
