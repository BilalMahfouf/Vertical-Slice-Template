using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Appointments;

/// <summary>Defines domain error codes and messages for appointment-related operations.</summary>
public static class AppointmentErrors
{
    /// <summary>Returned when an appointment slot conflicts with an existing booking for the same doctor.</summary>
    public static Error InvalidAppointmentDate
        => Error.Conflict("Appointment.InvalidAppointmentDate",
            "The doctor has an appointment at this time");

    /// <summary>Returned when attempting to reschedule a completed or cancelled appointment.</summary>
    public static Error RescheduleProblem
        => Error.Conflict($"Appointment.{nameof(RescheduleProblem)}",
            "Completed or Cancelled appointmemts can't be rescheduled");

    /// <summary>Returned when attempting to complete an appointment that is not in a confirmable or rescheduled state.</summary>
    public static Error CompleteProblem
        => Error.Conflict($"Appointment.{nameof(CompleteProblem)}",
            "Only confirmed or rescheduled  appointmemts can be completed");

    /// <summary>Returned when attempting to cancel an appointment that is not in a cancellable state.</summary>
    public static Error CancelProblem
        => Error.Conflict($"Appointment.{nameof(CancelProblem)}",
            "Only confirmed or rescheduled  appointmemts can be cancelled");

    /// <summary>Returned when an appointment with the specified <paramref name="id"/> cannot be found.</summary>
    public static Error NotFound(Guid id)
        => Error.NotFound($"Appointment.{nameof(NotFound)}",
            $"The appointment with the specified id: {id} was not found");

    /// <summary>Returned when an appointment with the specified <paramref name="appointmentId"/> cannot be found.</summary>
    public static Error AppointmentNotFound(Guid appointmentId)
        => Error.NotFound("Appointment.AppointmentNotFound",
            $"Appointment with id '{appointmentId}' was not found");

    /// <summary>Returned when no appointments exist for the current tenant.</summary>
    public static Error AppointmentsNotFound
        => Error.NotFound("Appointment.AppointmentsNotFound", "Appointments not found");

    /// <summary>Returned when the appointment status is incompatible with the requested operation.</summary>
    public static Error InvalidAppointmentStatus =>
                Error.Validation("Appointment.InvalidAppointmentStatus",
            "The appointment status is invalid for this operation");

    /// <summary>
    /// Returned when an appointment's date is in the past, causing a conflict with the current visit date.
    /// </summary>
    /// <param name="appointmentDate">The outdated appointment date.</param>
    public static Error OutDatedAppointment(DateTime appointmentDate)
        => Error.Conflict($"{nameof(Appointment)}.{nameof(OutDatedAppointment)}",
            $"The appointment date {appointmentDate} conflicts " +
            $"with the visit date {DateTime.UtcNow}. " +
            $"Appointments must not be outdated.");
}
