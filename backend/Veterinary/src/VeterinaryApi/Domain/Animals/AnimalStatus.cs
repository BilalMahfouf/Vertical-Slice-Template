namespace VeterinaryApi.Domain.Animals;

public enum AnimalStatus : byte
{
    Active = 1,
    UnderTreatment = 2,
    Recovered = 3,
    Critical = 4,
    Deceased = 5
}
