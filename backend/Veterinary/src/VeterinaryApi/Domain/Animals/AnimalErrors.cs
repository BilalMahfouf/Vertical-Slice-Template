using Org.BouncyCastle.Asn1.Mozilla;
using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Animals;

/// <summary>Defines domain error codes and messages for animal-related operations.</summary>
public static class AnimalErrors
{
    /// <summary>Returned when an animal with the specified <paramref name="animalId"/> cannot be found.</summary>
    public static Error AnimalNotFound(Guid animalId)
        => Error.NotFound("Animal.AnimalNotFound",
            $"Animal with id '{animalId}' was not found");

    /// <summary>Returned when a query for multiple animals returns an empty collection.</summary>
    public static Error AnimalsNotFound
        => Error.NotFound("Animal.AnimalsNotFound", "Animals not found");

    /// <summary>Returned when the supplied gender value is not a valid <c>Gender</c> enum member.</summary>
    public static Error InvalidGenderEnum
        => Error.Validation("Animal.InvalidGenderEnum",
            "The gender is invalid for this operation");

    /// <summary>Returned when the supplied status value is not a valid <c>AnimalStatus</c> enum member.</summary>
    public static Error InvalidAnimalStatus
        => Error.Validation("Animal.InvalidAnimalStatus",
            "The animal status is invalid for this operation");
}
