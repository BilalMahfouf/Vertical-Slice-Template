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
    public VisitType VisitType { get; private set; }
    public List<string>? Symptoms { get; private set; }
    public List<string>? Diagnosis { get; private set; }
    public List<string>? Treatment { get; private set; }
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
        VisitType visitType,
        List<string>? symptoms,
        List<string>? diagnosis,
        List<string>? treatment,
        string? followUpNotes
        )
    {
        var visit = new Visit();

        visit.AnimalId = animalId;
        visit.OwnerId = ownerId;
        visit.AppointmentId = appointmentId;
        visit.VisitType = visitType;

        visit.Symptoms = visit.GetStrings(symptoms);
        visit.Diagnosis = visit.GetStrings(diagnosis);
        visit.Treatment = visit.GetStrings(treatment);

        visit.Notes = string.IsNullOrWhiteSpace(followUpNotes) ? null : followUpNotes.Trim();

        return visit;
    }
    private List<string>? GetStrings(List<string>? strList)
    {
        return strList?
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .ToList();
    }

    public void UpdateDetails(
        VisitType visitType,
        List<string>? symptoms,
        List<string>? diagnosis,
        List<string>? treatment,
        string? followUpNotes)
    {
        VisitType = visitType;
        Symptoms = GetStrings(symptoms);
        Diagnosis = GetStrings(diagnosis);
        Treatment = GetStrings(treatment);
        Notes = string.IsNullOrWhiteSpace(followUpNotes) ? null : followUpNotes.Trim();
    }


}
