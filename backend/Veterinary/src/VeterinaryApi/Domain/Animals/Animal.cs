using VeterinaryApi.Domain.Clients;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Animals;

public class Animal : Entity
{
    public Guid ClinicId { get; private set; }

    public Guid ClientId { get; private set; }

    public string Name { get; private set; } = null!;

    public string Species { get; private set; } = null!;

    public string? Breed { get; private set; } = null!;

    public Gender Gender { get; private set; }

    public DateTime? BirthDate { get; private set; }

    public AnimalStatus Status { get; private set; }

    public string? Color { get; private set; } = null!;

    public string? MicrochipNumber { get; private set; }

    public DateTime? UpdatedOnUtc { get; private set; }

    public Clinic Clinic { get; private set; } = null!;

    public Client Client { get; private set; } = null!;

    private Animal()
    {
    }

    public static Animal Create(
        Guid clinicId,
        Guid clientId,
        string name,
        string species,
        string? breed,
        Gender gender,
        DateTime? birthDate,
        string? color,
        AnimalStatus status,
        string? microchipNumber = null)
    {
        var animal = new Animal();
        animal.ClinicId = clinicId;
        animal.ClientId = clientId;
        animal.Name = name.Trim();
        animal.Species = species.Trim();
        animal.Breed = string.IsNullOrWhiteSpace(breed) ? null : breed.Trim();
        animal.Gender = gender;
        animal.BirthDate = birthDate;
        animal.Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        animal.MicrochipNumber = string.IsNullOrWhiteSpace(microchipNumber)
            ? null : microchipNumber.Trim();
        animal.Status = status;

        return animal;
    }

    public void UpdateDetails(
        string name,
        string species,
        string breed,
        Gender gender,
        DateTime? birthDate,
        string? color,
        string? microchipNumber = null)
    {
        Name = name.Trim();
        Species = species.Trim();
        Breed = string.IsNullOrWhiteSpace(breed) ? null : breed.Trim();
        Gender = gender;
        BirthDate = birthDate;
        Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        MicrochipNumber = string.IsNullOrWhiteSpace(microchipNumber)
            ? null : microchipNumber.Trim();
        UpdatedOnUtc = DateTime.UtcNow;
    }
    public void UpdateStatus(AnimalStatus status)
    {
        this.Status = status;
    }
}
