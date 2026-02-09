using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Clinics;

public static class ClinicErrors
{
    public static Error InvalidClinicName(int nameLength)
        => Error.Conflict("Clinic.InvalidClinicName",
            $"Clinic name must be ${nameLength} letters at least");
    public static Error ClinicNotFound(Guid clinicId)
        => Error.NotFound("Clinic.ClinicNotFound",
            $"Clinic with id '{clinicId}' was not found");
    public static Error ClinicNotFound()
        => Error.NotFound("Clinic.ClinicNotFound",
            $"Clinic  was not found");

    public static Error ClinicsNotFound
        => Error.NotFound("Clinic.ClinicsNotFound", "Clinics not found");
    public static Error ClinicNotFoundForUser(Guid userId)
        => Error.NotFound("Clinic.ClinicNotFoundForUser",
            $"Clinic for user with id '{userId}' was not found");
}
