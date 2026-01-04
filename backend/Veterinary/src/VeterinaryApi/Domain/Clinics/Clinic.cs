using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Clinics;

public class Clinic : Entity
{

    public Guid DoctorId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string Address { get; private set; } = null!;

    // Required by EF Core
    private Clinic() { }

    private static int _minNameLength = 3;
    public static Clinic Create(
        Guid doctorId,
        string name,
        string phone,
        string address)
    {
        if(name.Length <_minNameLength)
        {
            throw new DomainException(ClinicErrors.InvalidClinicName(_minNameLength));
        }
        var clinic = new Clinic
        {
            DoctorId = doctorId,
            Name = name,
            Phone = phone,
            Address = address
        };
        return clinic;
    }

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
