using System.Diagnostics;
using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Appointments;

public sealed class Appointment : Entity
{
    public Guid ClinicId { get; private set; }
    public Guid AnimalId { get; private set; }
    public DateTime AppointmentDate { get; private set; }
    public TimeSpan AppointmentTime { get; private set; }
    public string Location { get; private set; } = null!;
    public AppointmentStatus Status { get; private set; }
    public DateTime? StatusUpdatedOnUtc { get; private set; }
    public string? Notes { get; private set; }

    public Clinic Clinic { get; private set; } = null!;
    public Animal Animal { get; private set; } = null!;

    public static Appointment Create(
        Guid animalId,
        Guid clinicId,
        DateTime appointmentDate,
        TimeSpan appointmentTime,
        string location,
        string? notes)
    {
        var appointment = new Appointment();
        appointment.AnimalId = animalId;
        appointment.ClinicId = clinicId;
        appointment.AppointmentDate = appointmentDate;
        appointment.AppointmentTime = appointmentTime;
        appointment.Location = location;
        appointment.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        appointment.Status = AppointmentStatus.Confirmed;

        return appointment;
    }

    private void UpdateStatus(AppointmentStatus status)
    {
        this.Status = AppointmentStatus.Rescheduled;
        this.StatusUpdatedOnUtc = DateTime.UtcNow;

    }
    public void Reschedule(DateTime newAppointmentDate)
    {
        if (this.Status is AppointmentStatus.Cancelled ||
            this.Status is AppointmentStatus.Completed)
        {
            throw new DomainException(AppointmentErrors.RescheduleProblem);
        }
        this.AppointmentDate = newAppointmentDate;
        UpdateStatus(AppointmentStatus.Rescheduled);
    }
    public void Complete()
    {
        if (this.Status is not AppointmentStatus.Confirmed ||
            this.Status is not AppointmentStatus.Rescheduled)
        {
            throw new DomainException(AppointmentErrors.CompleteProblem);
        }
        UpdateStatus(AppointmentStatus.Completed);
    }
    
    public void Cancel(string?notes=null)
    {
        if(this.Status is not AppointmentStatus.Confirmed ||
            this.Status is not AppointmentStatus.Rescheduled)
        {
            throw new DomainException(AppointmentErrors.CancelProblem);
        }
        UpdateStatus(AppointmentStatus.Cancelled);
        notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
    }

}
