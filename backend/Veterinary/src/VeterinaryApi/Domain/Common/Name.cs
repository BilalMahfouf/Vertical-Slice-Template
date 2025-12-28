namespace VeterinaryApi.Domain.Common;

public class Name
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";

    private Name()
    {
    }

    private Name(string firstName, string lastName)
    {
        this.FirstName = firstName;
        this.LastName = lastName;
    }

    public static Name Create(string firstName, string lastName)
    {
        return new Name(firstName, lastName);
    }



}
