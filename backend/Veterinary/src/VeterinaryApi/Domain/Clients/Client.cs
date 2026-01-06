using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Clients;

public class Client : Entity
{
    public Guid ClinicId { get; private set; }
    public string FullName { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string? Notes { get; private set; }
    public DateTime? UpdatedOnUtc { get; private set; }
    public Clinic Clinic { get; private set; } = null!;

    private Client()
    {
    }

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
