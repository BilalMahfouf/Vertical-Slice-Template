using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Domain.Common;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Domain.Vaccinations;

/// <summary>
/// Aggregate representing a vaccination record for an animal.
/// Optionally linked to a <see cref="Visit"/> if administered during a clinical visit.
/// </summary>
public sealed class Vaccination : Entity
{
    /// <summary>Identifier of the animal that received the vaccination.</summary>
    public Guid AnimalId { get; private set; }

    /// <summary>Optional identifier of the visit during which the vaccination was administered.</summary>
    public Guid? VisitId { get; private set; }

    /// <summary>Name or type of the vaccine administered (e.g., "Rabies", "Distemper").</summary>
    public string Name { get; private set; } = null!;

    /// <summary>Date and time when the vaccine was given (UTC).</summary>
    public DateTime GivenAt { get; private set; }

    /// <summary>Optional next-due date for a booster dose.</summary>
    public DateTime? DueTo { get; private set; }

    /// <summary>Optional clinical notes about this vaccination.</summary>
    public string? Notes { get; private set; }

    /// <summary>Navigation property to the vaccinated animal.</summary>
    public Animal Animal { get; private set; } = null!;

    /// <summary>Navigation property to the associated visit, if any.</summary>
    public Visit? Visit { get; private set; } = null;

    private Vaccination() { }

    /// <summary>Factory method that creates a new vaccination record.</summary>
    public static Vaccination Create(
        Guid animalId,
        Guid? visitId,
        string name,
        DateTime givenAt,
        DateTime? dueTo,
        string? notes = null)
    {
        var vaccination = new Vaccination
        {
            AnimalId = animalId,
            Name = name,
            GivenAt = givenAt,
            DueTo = dueTo,
            Notes = notes
        };
        vaccination.VisitId = visitId is null ? null : visitId.Value;
        return vaccination;
    }

    /// <summary>Updates the vaccination's name, dates, and notes.</summary>
    public void Update(
         string name,
         DateTime givenAt,
         DateTime? dueTo,
         string? notes = null)
    {
        Name = name;
        GivenAt = givenAt;
        DueTo = dueTo;
        Notes = notes;
    }

}
