namespace VeterinaryApi.Domain.Visits;

/// <summary>
/// Categorizes the physical setting or urgency level of a veterinary <see cref="Visit"/>.
/// </summary>
public enum VisitType : byte
{
    /// <summary>A routine visit performed at the veterinary clinic premises.</summary>
    Clinic = 1,

    /// <summary>An on-site visit performed at the animal owner's location (farm, home, etc.).</summary>
    Field,

    /// <summary>An unscheduled urgent visit due to a medical emergency.</summary>
    Emergency,
}
