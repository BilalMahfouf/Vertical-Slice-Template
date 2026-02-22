using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Visits;

/// <summary>
/// Centralised error definitions for the <see cref="Visit"/> domain.
/// All errors follow the <c>{Aggregate}.{ErrorName}</c> naming convention.
/// </summary>
public static class VisitErrors
{
    /// <summary>Returned when a visit lookup by no specific criteria yields no result.</summary>
    public static Error VisitNotFound()
        => Error.NotFound(
        "Visit.VisitNotFound",
        "Visit not found.");

    /// <summary>Returned when a visit lookup by <paramref name="id"/> yields no result.</summary>
    /// <param name="id">The identifier that was searched for.</param>
    public static Error VisitNotFound(Guid id)
        => Error.NotFound(
        "Visit.VisitNotFound",
        $"Visit with id {id} was not found.");

    /// <summary>Returned when a query for multiple visits returns an empty collection.</summary>
    public static Error VisitsNotFound = Error.NotFound(
        "Visit.VisitsNotFound",
        "No visits found.");

    /// <summary>
    /// Returned when an attempt is made to create a second visit linked to an appointment
    /// that already has an associated visit record.
    /// </summary>
    /// <param name="appointmentId">The appointment that already has a visit.</param>
    public static Error VisitWithAppointmentlAlreadyExist(Guid appointmentId)
        => Error.Conflict(
            $"Visit.{nameof(VisitWithAppointmentlAlreadyExist)}",
            $"The Appointment with id {appointmentId} already have a visit");

    /// <summary>Returned when the provided payment amount is below the minimum allowed value.</summary>
    public static Error InvalidPaymentAmount
        => Error.Conflict($"{nameof(Visit)}s.{nameof(InvalidPaymentAmount)}",
            $"The payment amount must be greater then {Visit.MinPaymentAmount}");

    /// <summary>Returned when an attempt is made to update payment on a visit that is already fully paid.</summary>
    public static Error PaymentAlreadyPayed
        => Error.Conflict($"{nameof(Visit)}s.{nameof(PaymentAlreadyPayed)}",
            "This payment is already payed");
}
