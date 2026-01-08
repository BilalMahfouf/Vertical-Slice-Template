using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Domain.Clients;

public static class ClientErrors
{
    public static Error ClientNotFound(Guid clientId)
        => Error.NotFound("Client.ClientNotFound",
            $"Client with id '{clientId}' was not found");
    public static Error ClientsNotFound
        => Error.NotFound("Client.ClientsNotFound", "Clients not found");
}
