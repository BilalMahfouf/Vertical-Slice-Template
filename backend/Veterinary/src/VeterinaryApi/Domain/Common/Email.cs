namespace VeterinaryApi.Domain.Common;

public class Email
{
    public string Address { get; set; } = string.Empty;

    private Email()
    { 
    }
    private Email(string address)
    {
       this.Address = address; 
    }

    public static Email Create(string address)
    {
        // to do add email validation here
        return new Email(address);
    }
}
