using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Visits;

public static class VisitErrors
{
    public static Error VisitNotFound = Error.NotFound(
        "Visit.VisitNotFound",
        "Visit not found.");

    public static Error VisitsNotFound = Error.NotFound(
        "Visit.VisitsNotFound",
        "No visits found.");
}
