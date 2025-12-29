using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Users;

public class User
{
    public Guid Id { get; private set; }

    public string UserName {  get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRoles Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; set; }

    private User()
    {
    }

    public static User Create(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        UserRoles role)
    {

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName= lastName,
            PasswordHash = passwordHash,
            Email = email,
            Role = role,
            IsActive = true
        };
        return user;
    }
}
