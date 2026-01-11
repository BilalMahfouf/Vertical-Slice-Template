using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Clients;
using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Visits;

public class Visit : Entity
{
    public Guid AnimalId { get; private set; }

    public Guid OwnerId { get; private set; }

    public Guid? AppointmentId { get; private set; }

    public DateTime VisitDate { get; private set; }

    public string? Symptoms { get; private set; }

    public string? Diagnosis { get; private set; }

    public string? Treatment { get; private set; }

    public string? Notes { get; private set; }

    public Animal Animal { get; private set; } = null!;

    public Client Owner { get; private set; } = null!;

    public Appointment? Appointment { get; private set; }

    private Visit()
    {
    }

    public static Visit Create(
        Guid animalId,
        Guid ownerId,
        Guid? appointmentId,
        DateTime visitDate,
        string? symptoms,
        string? diagnosis,
        string? treatment,
        string? followUpNotes
        )
    {
        var visit = new Visit();

        visit.AnimalId = animalId;
        visit.OwnerId = ownerId;
        visit.AppointmentId = appointmentId;
        visit.VisitDate = visitDate.ToUniversalTime();
        visit.Symptoms = string.IsNullOrWhiteSpace(symptoms) ? null : symptoms.Trim();
        visit.Diagnosis = string.IsNullOrWhiteSpace(diagnosis) ? null : diagnosis.Trim();
        visit.Treatment = string.IsNullOrWhiteSpace(treatment) ? null : treatment.Trim();
        visit.Notes = string.IsNullOrWhiteSpace(followUpNotes) ? null : followUpNotes.Trim();

        return visit;
    }

    public void UpdateDetails(
        string? symptoms,
        string? diagnosis,
        string? treatment,
        string? followUpNotes)
    {
        Symptoms = string.IsNullOrWhiteSpace(symptoms) ? null : symptoms.Trim();
        Diagnosis = string.IsNullOrWhiteSpace(diagnosis) ? null : diagnosis.Trim();
        Treatment = string.IsNullOrWhiteSpace(treatment) ? null : treatment.Trim();
        Notes = string.IsNullOrWhiteSpace(followUpNotes) ? null : followUpNotes.Trim();
    }


}
