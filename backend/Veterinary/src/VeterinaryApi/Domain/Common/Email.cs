namespace VeterinaryApi.Domain.Common;

public class Email
{
    public string Value { get; set; } = string.Empty;

    private Email()
    { 
    }
    private Email(string value)
    {
       this.Value = value; 
    }

    public static Email Create(string address)
    {
        // to do add email validation here
        return new Email(address);
    }
}
