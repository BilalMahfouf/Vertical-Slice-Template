namespace VeterinaryApi.Features.Clients;


public sealed record ClientReadResponse(
        Guid Id,
        Guid ClinicId,
        string ClinicName,
        string FullName,
        string Phone,
        string? Notes,
        DateTime CreatedOnUtc,
        int NumberOfAnimals);
