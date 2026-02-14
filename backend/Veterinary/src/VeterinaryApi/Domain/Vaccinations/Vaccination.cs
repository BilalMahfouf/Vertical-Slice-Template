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
    public string? Notes { get; private set; }

    public Animal Animal { get; private set; } = null!;
    public Visit? Visit { get; private set; } = null;

    public static Vaccination Create(
        Guid animalId,
        string name,
        DateTime givenAt,
        string? notes = null)
    {
        return new Vaccination
        {
            AnimalId = animalId,
            Name = name,
            GivenAt = givenAt,
            Notes = notes
        };
    }
    private Vaccination() { }

}
