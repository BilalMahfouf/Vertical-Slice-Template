using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Appointments;

public static class AppointmentErrors
{
    public static Error InvalidAppointmentDate
        => Error.Conflict("Appointment.InvalidAppointmentDate",
            "The doctor has an appointment at this time");
    public static Error RescheduleProblem
        => Error.Conflict($"Appointment.${nameof(RescheduleProblem)}",
            "Completed or Cancelled appointmemts can't be rescheduled");
    public static Error CompleteProblem
        => Error.Conflict($"Appointment.${nameof(CompleteProblem)}",
            "Only confirmed or rescheduled  appointmemts can be completed");
    public static Error CancelProblem
        => Error.Conflict($"Appointment.${nameof(CancelProblem)}",
            "Only confirmed or rescheduled  appointmemts can be cancelled");
    public static Error NotFound(Guid id)
        => Error.NotFound($"Appointment.{nameof(NotFound)}",
            $"The appointment with the specified id: {id} was not found");
    public static Error AppointmentNotFound(Guid appointmentId)
        => Error.NotFound("Appointment.AppointmentNotFound",
            $"Appointment with id '{appointmentId}' was not found");
    public static Error AppointmentsNotFound
        => Error.NotFound("Appointment.AppointmentsNotFound", "Appointments not found");
}
