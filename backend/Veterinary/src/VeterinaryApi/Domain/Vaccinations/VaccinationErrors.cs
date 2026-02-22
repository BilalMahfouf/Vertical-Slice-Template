using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Vaccinations;

/// <summary>Defines domain error codes and messages for vaccination-related operations.</summary>
public static class VaccinationErrors
{
    /// <summary>Returned when the requested vaccination record cannot be found.</summary>
    public static Error NotFound =>
        Error.NotFound(
            $"{nameof(Vaccination)}s.NotFound",
            "The specified vaccination was not found.");
}
