namespace VeterinaryApi.Domain.Animals;

/// <summary>Represents the health/treatment status of an animal in the veterinary system.</summary>
public enum AnimalStatus : byte
{
    /// <summary>Animal is in good health with no ongoing treatment.</summary>
    Active = 1,
    /// <summary>Animal is currently receiving treatment.</summary>
    UnderTreatment = 2,
    /// <summary>Animal has fully recovered from illness or treatment.</summary>
    Recovered = 3,
    /// <summary>Animal is in a critical health condition.</summary>
    Critical = 4,
    /// <summary>Animal is deceased.</summary>
    Deceased = 5
}
