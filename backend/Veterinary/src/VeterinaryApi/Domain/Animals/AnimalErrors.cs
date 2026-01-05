using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Animals;

public static class AnimalErrors
{
    public static Error AnimalNotFound(Guid animalId)
        => Error.NotFound("Animal.AnimalNotFound",
            $"Animal with id '{animalId}' was not found");
    public static Error AnimalsNotFound
        => Error.NotFound("Animal.AnimalsNotFound", "Animals not found");
}
