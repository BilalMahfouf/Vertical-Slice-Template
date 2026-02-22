namespace VeterinaryApi.Domain.Appointments;

/// <summary>
/// Defines the lifecycle states of an <see cref="Appointment"/>.
/// Transitions are enforced by the entity's business methods and guard against invalid state changes.
/// </summary>
public enum AppointmentStatus : byte
{
    /// <summary>The appointment has been booked and confirmed by the system. Initial state for all new appointments.</summary>
    Confirmed = 1,

    /// <summary>The appointment was cancelled. Terminal state — no further transitions allowed.</summary>
    Cancelled = 2,

    /// <summary>The appointment was completed successfully. Terminal state — no further transitions allowed.</summary>
    Completed = 3,

    /// <summary>The appointment was rescheduled to a new date/time. Can still be cancelled or completed.</summary>
    Rescheduled = 4,
}

