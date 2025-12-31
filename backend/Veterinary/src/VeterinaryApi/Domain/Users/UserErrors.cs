using Microsoft.CodeAnalysis.Operations;
using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Users;


public static class UserErrors
{
    public static Error UserNotFound(string email) =>
                Error.NotFound(
                    $"{nameof(User)}.NotFound",
                    $"User with email {email} is not found");
    public static Error UserNotFound(Guid id) =>
        Error.NotFound($"{nameof(User)}.NotFound",
            $"User with id {id} is not found");

    public static Error InvalidCredentials =>
                Error.Unauthorized(
                    $"{nameof(User)}.InvalidCredentials",
                    "The provided credentials are invalid");

    public static Error ExpiredRefreshToken =>
        Error.Conflict(
            $"{nameof(User)}.ExpiredRefreshToken",
            "Refresh Token is expired, please login again");
}

