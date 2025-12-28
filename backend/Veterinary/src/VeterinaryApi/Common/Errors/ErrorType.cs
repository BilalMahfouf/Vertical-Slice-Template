namespace VeterinaryApi.Common.Errors;

public enum ErrorType
{
    None = 1,
    BadRequest = 400,
    NotFound = 404,
    Unauthorized = 401,
    Conflict = 409,
    InternalServerError = 500
}
