using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Visits;

public static class VisitErrors
{
    public static Error VisitNotFound()
        => Error.NotFound(
        "Visit.VisitNotFound",
        "Visit not found.");
    public static Error VisitNotFound(Guid id)
        => Error.NotFound(
        "Visit.VisitNotFound",
        $"Visit with id {id} was not found.");

    public static Error VisitsNotFound = Error.NotFound(
        "Visit.VisitsNotFound",
        "No visits found.");

    public static Error VisitWithAppointmentlAlreadyExist(Guid appointmentId)
        => Error.Conflict(
            $"Visit.{nameof(VisitWithAppointmentlAlreadyExist)}",
            $"The Appointment with id {appointmentId} already have a visit");

    public static Error InvalidPaymentAmount
        => Error.Conflict($"{nameof(Visit)}s.{nameof(InvalidPaymentAmount)}",
            $"The payment amount must be greater then {Visit.MinPaymentAmount}");
    public static Error PaymentAlreadyPayed
        => Error.Conflict($"{nameof(Visit)}s.{nameof(PaymentAlreadyPayed)}",
            "This payment is already payed");
}
