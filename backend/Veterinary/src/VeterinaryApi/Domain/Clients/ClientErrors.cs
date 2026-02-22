using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Clients;

/// <summary>Defines domain error codes and messages for client (pet owner) related operations.</summary>
public static class ClientErrors
{
    /// <summary>Returned when a client with the specified <paramref name="clientId"/> cannot be found.</summary>
    public static Error ClientNotFound(Guid clientId)
        => Error.NotFound("Client.ClientNotFound",
            $"Client with id '{clientId}' was not found");

    /// <summary>Returned when a client cannot be found without a specific identifier.</summary>
    public static Error ClientNotFound()
           => Error.NotFound("Client.ClientNotFound",
               $"Client  was not found");

    /// <summary>Returned when a query for multiple clients returns an empty collection.</summary>
    public static Error ClientsNotFound
        => Error.NotFound("Client.ClientsNotFound", "Clients not found");

    /// <summary>Returned when a client with the same <paramref name="fullName"/> and <paramref name="phone"/> already exists.</summary>
    public static Error DuplicateClient(string fullName, string phone)
        => Error.Conflict("Client.DuplicateClient",
            $"Client with name '{fullName}' and phone '{phone}' already exists");

    /// <summary>Returned when a client with the same <paramref name="fullName"/> already exists (name-only duplicate check).</summary>
    public static Error ClientWithSameNameExists(string fullName)
        => Error.Conflict("Client.ClientWithSameNameExists",
            $"Client with name '{fullName}' already exists");
}
