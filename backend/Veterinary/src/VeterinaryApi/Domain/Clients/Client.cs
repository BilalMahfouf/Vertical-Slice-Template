using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Domain.Common;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Domain.Clients;

/// <summary>
/// Represents a pet owner (client) registered at a veterinary clinic.
/// Clients are the human owners of animals and serve as the contact point
/// for all animal-related communications and billing.
/// </summary>
/// <remarks>
/// The full name is stored as a single concatenated string derived from first and last name.
/// This simplifies querying and searching while still allowing structured input via the API.
/// </remarks>
public class Client : Entity
{
    /// <summary>Gets the identifier of the clinic this client is registered with.</summary>
    public Guid ClinicId { get; private set; }

    /// <summary>Gets the full name of the client, stored as "FirstName LastName".</summary>
    public string FullName { get; private set; } = null!;

    /// <summary>Gets the client's contact phone number.</summary>
    public string Phone { get; private set; } = null!;

    /// <summary>Gets any additional notes about the client (optional).</summary>
    public string? Notes { get; private set; }

    /// <summary>Gets the UTC timestamp of the last profile update, or <c>null</c> if never updated.</summary>
    public DateTime? UpdatedOnUtc { get; private set; }

    /// <summary>Navigation property to the associated clinic. Populated by EF Core.</summary>
    public Clinic Clinic { get; private set; } = null!;

    /// <summary>
    /// Navigation property to the list of animals owned by this client.
    /// Populated by EF Core when explicitly included in a query.
    /// </summary>
    public IReadOnlyCollection<Animal> Animals { get; private set; } = null!;

    public IReadOnlyCollection<Visit> Visits { get; private set; } = null!;

    private Client()
    {
    }

    /// <summary>
    /// Factory method that creates a new client registered under the specified clinic.
    /// The full name is composed by concatenating first and last name.
    /// </summary>
    /// <param name="clinicId">The ID of the clinic this client belongs to.</param>
    /// <param name="firstName">The client's first name.</param>
    /// <param name="lastName">The client's last name.</param>
    /// <param name="phone">The client's contact phone number (trimmed).</param>
    /// <param name="notes">Optional notes about the client (null/whitespace becomes null).</param>
    /// <returns>A new <see cref="Client"/> instance.</returns>
    public static Client Create(
        Guid clinicId,
        string firstName,
        string lastName,
        string phone,
        string? notes = null)
    {

        var owner = new Client
        {
            ClinicId = clinicId,
            FullName = $"{firstName} {lastName}",
            Phone = phone.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };
        return owner;
    }

    /// <summary>
    /// Updates the client's contact details and records the change timestamp.
    /// </summary>
    /// <param name="firstName">The new first name.</param>
    /// <param name="lastName">The new last name.</param>
    /// <param name="phone">The new phone number (trimmed).</param>
    /// <param name="notes">New optional notes (null/whitespace becomes null).</param>
    public void UpdateDetails(
        string firstName,
        string lastName,
        string phone,
        string? notes = null)
    {
        FullName = $"{firstName} {lastName}";
        Phone = phone.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedOnUtc = DateTime.UtcNow;
    }
}
