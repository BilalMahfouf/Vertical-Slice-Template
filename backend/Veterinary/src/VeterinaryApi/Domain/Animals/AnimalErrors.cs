using Org.BouncyCastle.Asn1.Mozilla;
using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Animals;

public static class AnimalErrors
{
    public static Error AnimalNotFound(Guid animalId)
        => Error.NotFound("Animal.AnimalNotFound",
            $"Animal with id '{animalId}' was not found");
    public static Error AnimalsNotFound
        => Error.NotFound("Animal.AnimalsNotFound", "Animals not found");

    public static Error InvalidGenderEnum
        => Error.Validation("Animal.InvalidGenderEnum",
            "The gender is invalid for this operation");
    public static Error InvalidAnimalStatus 
        => Error.Validation("Animal.InvalidAnimalStatus",
            "The animal status is invalid for this operation");
}
