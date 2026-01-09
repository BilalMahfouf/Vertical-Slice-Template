namespace VeterinaryApi.Domain.Appointments;

public enum AppointmentStatus : byte
{
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
    Rescheduled = 4,

}

