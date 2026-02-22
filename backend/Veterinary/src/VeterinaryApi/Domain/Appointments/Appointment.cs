using System.Diagnostics;
using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Appointments;

/// <summary>
/// Represents a scheduled veterinary appointment for an animal at a clinic.
/// This is an aggregate root that enforces all appointment-related business rules
/// including status transitions and lifecycle management.
/// </summary>
/// <remarks>
/// An appointment always starts in the <see cref="AppointmentStatus.Confirmed"/> state when created.
/// It can then transition to <see cref="AppointmentStatus.Rescheduled"/>,
/// <see cref="AppointmentStatus.Completed"/>, or <see cref="AppointmentStatus.Cancelled"/>.
/// Cancellation raises an <see cref="AppointmentCancelledDomainEvent"/> that triggers
/// a real-time notification to the clinic owner.
/// </remarks>
public sealed class Appointment : Entity
{
    /// <summary>Gets the identifier of the clinic where this appointment is scheduled.</summary>
    public Guid ClinicId { get; private set; }

    /// <summary>Gets the identifier of the animal this appointment is for.</summary>
    public Guid AnimalId { get; private set; }

    /// <summary>Gets the scheduled UTC date and time of the appointment.</summary>
    public DateTime AppointmentDate { get; private set; }

    /// <summary>Gets the physical or descriptive location for the appointment, if provided.</summary>
    public string? Location { get; private set; } = null!;

    /// <summary>Gets the current status of the appointment in its lifecycle.</summary>
    public AppointmentStatus Status { get; private set; }

    /// <summary>Gets the UTC timestamp when the status was last updated, or <c>null</c> if never changed.</summary>
    public DateTime? StatusUpdatedOnUtc { get; private set; }

    /// <summary>Gets any additional notes associated with the appointment.</summary>
    public string? Notes { get; private set; }

    /// <summary>Navigation property to the associated clinic. Populated by EF Core.</summary>
    public Clinic Clinic { get; private set; } = null!;

    /// <summary>Navigation property to the associated animal. Populated by EF Core.</summary>
    public Animal Animal { get; private set; } = null!;

    /// <summary>
    /// Factory method that creates a new appointment in the <see cref="AppointmentStatus.Confirmed"/> state.
    /// Use this method rather than the constructor to ensure invariants are enforced.
    /// </summary>
    /// <param name="animalId">The ID of the animal being seen.</param>
    /// <param name="clinicId">The ID of the clinic hosting the appointment.</param>
    /// <param name="appointmentDate">The scheduled date/time (converted to UTC internally).</param>
    /// <param name="location">The appointment location (optional).</param>
    /// <param name="notes">Any additional notes (optional; null/whitespace becomes null).</param>
    /// <returns>A new <see cref="Appointment"/> instance in <c>Confirmed</c> status.</returns>
    public static Appointment Create(
        Guid animalId,
        Guid clinicId,
        DateTime appointmentDate,
        string? location,
        string? notes)
    {

        var appointment = new Appointment();
        appointment.AnimalId = animalId;
        appointment.ClinicId = clinicId;
        appointment.AppointmentDate = appointmentDate.ToUniversalTime();
        appointment.Location = location;
        appointment.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        appointment.Status = AppointmentStatus.Confirmed;

        return appointment;
    }

    /// <summary>
    /// Internal helper that updates the appointment status and records the change timestamp.
    /// Validates that the provided status value is a defined enum member.
    /// </summary>
    /// <param name="status">The new status to transition to.</param>
    private void UpdateStatus(AppointmentStatus status)
    {
        ValidateStatusEnum(status);
        this.Status = status;
        this.StatusUpdatedOnUtc = DateTime.UtcNow;

    }

    /// <summary>
    /// Reschedules the appointment to a new date and transitions the status to
    /// <see cref="AppointmentStatus.Rescheduled"/>.
    /// </summary>
    /// <param name="newAppointmentDate">The new appointment date/time (converted to UTC internally).</param>
    /// <exception cref="DomainException">
    /// Thrown when the appointment is in <c>Cancelled</c> or <c>Completed</c> status,
    /// as rescheduling a terminal state is not permitted.
    /// </exception>
    public void Reschedule(DateTime newAppointmentDate)
    {
        if (this.Status is AppointmentStatus.Cancelled or
            AppointmentStatus.Completed)
        {
            throw new DomainException(AppointmentErrors.RescheduleProblem);
        }
        this.AppointmentDate = newAppointmentDate.ToUniversalTime();
        UpdateStatus(AppointmentStatus.Rescheduled);
    }

    /// <summary>
    /// Marks the appointment as completed. Only appointments in
    /// <see cref="AppointmentStatus.Confirmed"/> or <see cref="AppointmentStatus.Rescheduled"/>
    /// can be completed.
    /// </summary>
    /// <exception cref="DomainException">
    /// Thrown when the appointment is in any status other than <c>Confirmed</c> or <c>Rescheduled</c>.
    /// </exception>
    public void Complete()
    {
        if (this.Status is not (AppointmentStatus.Confirmed or
             AppointmentStatus.Rescheduled))
        {
            throw new DomainException(AppointmentErrors.CompleteProblem);
        }
        UpdateStatus(AppointmentStatus.Completed);
    }

    /// <summary>
    /// Cancels the appointment and raises an <see cref="AppointmentCancelledDomainEvent"/>.
    /// The domain event triggers a real-time notification to the clinic owner via SignalR.
    /// Only appointments in <see cref="AppointmentStatus.Confirmed"/> or
    /// <see cref="AppointmentStatus.Rescheduled"/> can be cancelled.
    /// </summary>
    /// <param name="notes">Optional cancellation notes (null/whitespace becomes null).</param>
    /// <exception cref="DomainException">
    /// Thrown when the appointment is already <c>Cancelled</c> or <c>Completed</c>.
    /// </exception>
    public void Cancel(string? notes = null)
    {
        if (this.Status is not
            (AppointmentStatus.Confirmed or AppointmentStatus.Rescheduled))
        {
            throw new DomainException(AppointmentErrors.CancelProblem);
        }
        UpdateStatus(AppointmentStatus.Cancelled);
        notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        RaiseDomainEvent(new AppointmentCancelledDomainEvent(this.Id));
    }

    private void ValidateStatusEnum(AppointmentStatus status)
    {
        if (!Enum.IsDefined(typeof(AppointmentStatus), status))
        {
            throw new DomainException(AppointmentErrors.InvalidAppointmentStatus);
        }
    }

}
