namespace VeterinaryApi.Features.Clients;

/// <summary>
/// Shared read-model DTO used across multiple client query endpoints.
/// Represents a client (pet owner) row with aggregate animal count.
/// </summary>
public sealed record ClientReadResponse(
        Guid Id,
        Guid ClinicId,
        string ClinicName,
        string FullName,
        string Phone,
        string? Notes,
        DateTime CreatedOnUtc,
        int NumberOfAnimals);
