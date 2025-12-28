namespace VeterinaryApi.Common.Errors;
public sealed class Error
{
    public string Code { get; private set; }
    public string Message { get; private set; }
    public ErrorType Type { get; private set; }
    public Error(ErrorType type, string code, string message)
    {
        Type = type;
        Code = code;
        Message = message;
    }
}
