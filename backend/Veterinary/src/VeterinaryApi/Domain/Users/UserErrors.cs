using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Users;


public static class UserErrors
{
    public static Error UserNotFound(string email) =>
                new Error(
                    ErrorType.NotFound,
                    "User.NotFound",
                    $"User with email {email} is not found");

    public static Error InvalidCredentials() =>
                new Error(
                    ErrorType.Unauthorized,
                    "User.InvalidCredentials",
                    "The provided credentials are invalid");
}
