using VeterinaryApi.Domain.Common;
using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Domain.Clinics;

/// <summary>
/// Represents a veterinary clinic owned and operated by a doctor (user).
/// Acts as the organizational unit for all client and animal data within the system.
/// Each doctor may own exactly one clinic; the clinic's <c>DoctorId</c> links it to its owner.
/// </summary>
/// <remarks>
/// A clinic name must be at least <c>3</c> characters long. This rule is enforced both
/// on creation (via <see cref="Create"/>) and when updating details (via <see cref="UpdateDetails"/>).
/// </remarks>
public class Clinic : Entity
{
    /// <summary>Gets the identifier of the doctor who owns and operates this clinic.</summary>
    public Guid DoctorId { get; private set; }

    /// <summary>Gets the display name of the clinic (minimum 3 characters).</summary>
    public string Name { get; private set; } = null!;

    /// <summary>Gets the contact phone number for the clinic.</summary>
    public string Phone { get; private set; } = null!;

    /// <summary>Gets the physical address of the clinic.</summary>
    public string Address { get; private set; } = null!;

    /// <summary>Gets the total number of staff members at this clinic.</summary>
    public int StaffCount { get; private set; }

    /// <summary>Navigation property to the owning doctor. Populated by EF Core.</summary>
    public User Doctor { get; private set; } = null!;

    /// <summary>Required by EF Core for proxies and change tracking. Not for external use.</summary>
    private Clinic() { }

    private static readonly int _minNameLength = 3;

    /// <summary>
    /// Factory method that creates a new clinic with the specified owner and details.
    /// Enforces the minimum name length business rule.
    /// </summary>
    /// <param name="doctorId">The ID of the doctor who will own this clinic.</param>
    /// <param name="name">The clinic name (must be at least 3 characters).</param>
    /// <param name="phone">The clinic contact phone number.</param>
    /// <param name="address">The clinic physical address.</param>
    /// <param name="staffCount">The number of staff members at this clinic.</param>
    /// <returns>A new <see cref="Clinic"/> instance.</returns>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="name"/> is shorter than the minimum required length (3 characters).
    /// </exception>
    public static Clinic Create(
        Guid doctorId,
        string name,
        string phone,
        string address,
        int staffCount)
    {
        if (name.Length < _minNameLength)
        {
            throw new DomainException(ClinicErrors.InvalidClinicName(_minNameLength));
        }
        var clinic = new Clinic
        {
            DoctorId = doctorId,
            Name = name,
            Phone = phone,
            Address = address,
            StaffCount = staffCount

        };
        return clinic;
    }

    /// <summary>
    /// Updates the clinic's editable details. The same minimum name length rule applies.
    /// The <c>DoctorId</c> and <c>StaffCount</c> are not changed by this method.
    /// </summary>
    /// <param name="name">The new clinic name (must be at least 3 characters).</param>
    /// <param name="phone">The new contact phone number.</param>
    /// <param name="address">The new physical address.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="name"/> is shorter than the minimum required length.
    /// </exception>
    public void UpdateDetails(string name, string phone, string address)
    {
        if (name.Length < _minNameLength)
        {
            throw new DomainException(ClinicErrors.InvalidClinicName(_minNameLength));
        }
        Name = name;
        Phone = phone;
        Address = address;
    }

}
