using VeterinaryApi.Domain.Clients;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Animals;

/// <summary>
/// Represents a patient animal registered at a veterinary clinic.
/// An animal is always associated with exactly one client (owner) and one clinic.
/// It is the central entity around which appointments, visits, vaccinations,
/// and prescriptions are organized.
/// </summary>
/// <remarks>
/// All string properties are trimmed on assignment to ensure consistent data.
/// Optional fields (Breed, Color, MicrochipNumber) are stored as <c>null</c>
/// when not provided rather than as empty strings.
/// </remarks>
public class Animal : Entity
{
    /// <summary>Gets the identifier of the clinic this animal is registered with.</summary>
    public Guid ClinicId { get; private set; }

    /// <summary>Gets the identifier of the client (owner) this animal belongs to.</summary>
    public Guid ClientId { get; private set; }

    /// <summary>Gets the name of the animal (e.g., "Buddy").</summary>
    public string Name { get; private set; } = null!;

    /// <summary>Gets the species of the animal (e.g., "Dog", "Cat", "Bird").</summary>
    public string Species { get; private set; } = null!;

    /// <summary>Gets the breed of the animal, or <c>null</c> if not specified.</summary>
    public string? Breed { get; private set; } = null!;

    /// <summary>Gets the biological gender of the animal.</summary>
    public Gender Gender { get; private set; }

    /// <summary>Gets the birth date of the animal, or <c>null</c> if unknown.</summary>
    public DateTime? BirthDate { get; private set; }

    /// <summary>Gets the current health/registration status of the animal.</summary>
    public AnimalStatus Status { get; private set; }

    /// <summary>Gets the coat color description, or <c>null</c> if not provided.</summary>
    public string? Color { get; private set; } = null!;

    /// <summary>Gets the ISO-standard microchip number, or <c>null</c> if not microchipped.</summary>
    public string? MicrochipNumber { get; private set; }

    /// <summary>Gets the UTC timestamp of the last update to this animal's details.</summary>
    public DateTime? UpdatedOnUtc { get; private set; }

    /// <summary>Navigation property to the associated clinic. Populated by EF Core.</summary>
    public Clinic Clinic { get; private set; } = null!;

    /// <summary>Navigation property to the associated client (owner). Populated by EF Core.</summary>
    public Client Client { get; private set; } = null!;

    private Animal()
    {
    }

    /// <summary>Validates that the provided gender value is a defined <see cref="Gender"/> enum member.</summary>
    /// <exception cref="DomainException">Thrown for undefined enum values.</exception>
    private void ValidateGenderEnum(Gender gender)
    {
        if (!Enum.IsDefined(typeof(Gender), gender))
        {
            throw new DomainException(AnimalErrors.InvalidGenderEnum);
        }
    }

    /// <summary>Validates that the provided status value is a defined <see cref="AnimalStatus"/> enum member.</summary>
    /// <exception cref="DomainException">Thrown for undefined enum values.</exception>
    private void ValidateAnimalStatusEnum(AnimalStatus status)
    {
        if (!Enum.IsDefined(typeof(AnimalStatus), status))
        {
            throw new DomainException(AnimalErrors.InvalidAnimalStatus);
        }
    }

    /// <summary>
    /// Factory method that creates a new registered animal with validated properties.
    /// Validates the gender and status enum values before assignment.
    /// </summary>
    /// <param name="clinicId">The ID of the clinic registering the animal.</param>
    /// <param name="clientId">The ID of the animal's owner (client).</param>
    /// <param name="name">The animal's name (trimmed).</param>
    /// <param name="species">The animal's species (trimmed).</param>
    /// <param name="breed">The breed, or <c>null</c>/whitespace to omit.</param>
    /// <param name="gender">The animal's gender (<see cref="Gender"/> enum).</param>
    /// <param name="birthDate">The animal's birth date, or <c>null</c> if unknown.</param>
    /// <param name="color">The coat color, or <c>null</c>/whitespace to omit.</param>
    /// <param name="status">The initial registration status (<see cref="AnimalStatus"/> enum).</param>
    /// <param name="microchipNumber">The microchip number, or <c>null</c>/whitespace to omit.</param>
    /// <returns>A fully initialized <see cref="Animal"/> instance.</returns>
    /// <exception cref="DomainException">Thrown when gender or status values are invalid.</exception>
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

        animal.ValidateGenderEnum(gender);
        animal.ValidateAnimalStatusEnum(status);

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

    /// <summary>
    /// Updates all mutable details of the animal, including its status.
    /// Validates gender and status enum values before applying changes.
    /// </summary>
    /// <param name="name">The updated name (trimmed).</param>
    /// <param name="species">The updated species (trimmed).</param>
    /// <param name="breed">The updated breed, or whitespace/null to clear.</param>
    /// <param name="gender">The updated gender.</param>
    /// <param name="birthDate">The updated birth date, or <c>null</c>.</param>
    /// <param name="color">The updated coat color, or whitespace/null to clear.</param>
    /// <param name="status">The updated status.</param>
    /// <param name="microchipNumber">The updated microchip number, or whitespace/null to clear.</param>
    /// <exception cref="DomainException">Thrown when gender or status values are invalid.</exception>
    public void UpdateDetails(
        string name,
        string species,
        string breed,
        Gender gender,
        DateTime? birthDate,
        string? color,
        AnimalStatus status,
        string? microchipNumber = null)
    {
        ValidateGenderEnum(gender);
        Name = name.Trim();
        Species = species.Trim();
        Breed = string.IsNullOrWhiteSpace(breed) ? null : breed.Trim();
        Gender = gender;
        BirthDate = birthDate;
        Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        MicrochipNumber = string.IsNullOrWhiteSpace(microchipNumber)
            ? null : microchipNumber.Trim();
        UpdateStatus(status);
    }

    /// <summary>
    /// Internal helper that updates the animal's status and records the change timestamp.
    /// </summary>
    /// <param name="status">The new status value.</param>
    private void UpdateStatus(AnimalStatus status)
    {
        ValidateAnimalStatusEnum(status);
        this.Status = status;
        this.UpdatedOnUtc = DateTime.UtcNow;
    }
}
