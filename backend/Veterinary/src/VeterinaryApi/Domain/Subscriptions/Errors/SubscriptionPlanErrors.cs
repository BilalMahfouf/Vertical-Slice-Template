using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Subscriptions.Errors;

public static class SubscriptionPlanErrors
{
    public static Error SubscriptionPlanNotFound(Guid planId)
        => Error.NotFound(
            code: $"{nameof(SubscriptionPlan)}.NotFound",
            description: $"Subscription plan with id {planId} was not found."
        );
    public static Error SubscriptionPlanNotFound()
      => Error.NotFound(
          code: $"{nameof(SubscriptionPlan)}.NotFound",
          description: $"Subscription plan  was not found."
      );
    public static Error SubscriptionPlansNotFound
            => Error.NotFound(
                code: $"{nameof(SubscriptionPlan)}.NotFound",
                description: $"Subscription plans  are not found."
            );
    public static Error SubscriptionPlanNameNotUnique(string name) =>
            Error.Conflict(
                code: $"{nameof(SubscriptionPlan)}.AlreadyExists",
                description: $"Subscription plan with name {name} already exists."
            );


}
