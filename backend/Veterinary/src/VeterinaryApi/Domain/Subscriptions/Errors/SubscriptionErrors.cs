using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Subscriptions.Errors;

public static class SubscriptionErrors
{
    public static Error NotFound
        => Error.NotFound(
            $"{nameof(Subscription)}.{nameof(NotFound)}",
            "The subscription was not found.");

    public static Error SubscriptionAlreadyCancelled
        => Error.Conflict(
            $"{nameof(Subscription)}.{nameof(SubscriptionAlreadyCancelled)}",
            "The subscription is already cancelled.");
    public static Error SubscriptionNotActive
       => Error.Conflict(
           $"{nameof(Subscription)}.{nameof(SubscriptionNotActive)}",
           "The subscription is not active");
    public static Error SubscriptionNotInRenewableState
        => Error.Conflict(
            $"{nameof(Subscription)}.{nameof(SubscriptionNotInRenewableState)}",
            "Only an active, past due, or expired subscription can be renewed.");
    public static Error AlreadyExistAcitveSubscription
        => Error.Conflict($"{nameof(Subscription)}." +
            $"{nameof(AlreadyExistAcitveSubscription)}",
            "This User Already have a an active subscription");
    public static Error FailedToRetrieveCheckout(Guid paymentId)
        => Error.Failure(
            $"{nameof(Subscription)}.{nameof(FailedToRetrieveCheckout)}",
            $"Failed to retrieve checkout for payment with ID: {paymentId}");

}
