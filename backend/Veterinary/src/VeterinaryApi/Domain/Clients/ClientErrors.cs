using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Clients;

public static class ClientErrors
{
    public static Error ClientNotFound(Guid clientId)
        => Error.NotFound("Client.ClientNotFound",
            $"Client with id '{clientId}' was not found");
    public static Error ClientNotFound()
           => Error.NotFound("Client.ClientNotFound",
               $"Client  was not found");

    public static Error ClientsNotFound
        => Error.NotFound("Client.ClientsNotFound", "Clients not found");

    public static Error DuplicateClient(string fullName, string phone)
        => Error.Conflict("Client.DuplicateClient",
            $"Client with name '{fullName}' and phone '{phone}' already exists");
    public static Error ClientWithSameNameExists(string fullName)
        => Error.Conflict("Client.ClientWithSameNameExists",
            $"Client with name '{fullName}' already exists");
}
