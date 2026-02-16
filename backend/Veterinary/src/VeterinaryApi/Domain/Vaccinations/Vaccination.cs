using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Domain.Common;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Domain.Vaccinations;

public sealed class Vaccination : Entity
{
    public Guid AnimalId { get; private set; }
    public Guid? VisitId { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTime GivenAt { get; private set; }
    public DateTime? DueTo { get; private set; }
    public string? Notes { get; private set; }

    public Animal Animal { get; private set; } = null!;
    public Visit? Visit { get; private set; } = null;

    private Vaccination() { }
    public static Vaccination Create(
        Guid animalId,
        Guid? visitId,
        string name,
        DateTime givenAt,
        DateTime dueTo,
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
