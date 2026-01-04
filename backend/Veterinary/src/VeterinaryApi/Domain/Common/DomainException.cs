using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Common;

public class DomainException : Exception
{
    public DomainException()
    {
    }

    public DomainException(Error error) : base(error.Description)
    {
    }
}
