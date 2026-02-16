using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Vaccinations;

public static class VaccinationErrors
{
    public static Error NotFound =>
        Error.NotFound(
            $"{nameof(Vaccination)}s.NotFound",
            "The specified vaccination was not found.");
}
