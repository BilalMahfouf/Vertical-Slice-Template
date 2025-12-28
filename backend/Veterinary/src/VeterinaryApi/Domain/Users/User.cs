using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Users;

public class User
{
    public Guid Id { get; private set; }
    public Name Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRoles Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; set; }

    private User()
    {
    }

    public static User Create(
        Name name,
        Email email,
        string passwordHash,
        UserRoles role)
    {

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            PasswordHash = passwordHash,
            Email = email,
            Role = role,
        };
        return user;
    }
}
