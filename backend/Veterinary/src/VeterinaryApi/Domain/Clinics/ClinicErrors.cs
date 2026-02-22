using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Clinics;

/// <summary>Defines domain error codes and messages for clinic-related operations.</summary>
public static class ClinicErrors
{
    /// <summary>Returned when the proposed clinic name is shorter than the required minimum length.</summary>
    /// <param name="nameLength">The minimum required name length.</param>
    public static Error InvalidClinicName(int nameLength)
        => Error.Conflict("Clinic.InvalidClinicName",
            $"Clinic name must be ${nameLength} letters at least");

    /// <summary>Returned when a clinic with the specified <paramref name="clinicId"/> cannot be found.</summary>
    public static Error ClinicNotFound(Guid clinicId)
        => Error.NotFound("Clinic.ClinicNotFound",
            $"Clinic with id '{clinicId}' was not found");

    /// <summary>Returned when a clinic cannot be found without a specific identifier.</summary>
    public static Error ClinicNotFound()
        => Error.NotFound("Clinic.ClinicNotFound",
            $"Clinic  was not found");

    /// <summary>Returned when a query for multiple clinics returns an empty collection.</summary>
    public static Error ClinicsNotFound
        => Error.NotFound("Clinic.ClinicsNotFound", "Clinics not found");

    /// <summary>Returned when a clinic associated with the specified <paramref name="userId"/> cannot be found.</summary>
    public static Error ClinicNotFoundForUser(Guid userId)
        => Error.NotFound("Clinic.ClinicNotFoundForUser",
            $"Clinic for user with id '{userId}' was not found");
}
