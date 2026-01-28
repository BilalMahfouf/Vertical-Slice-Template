namespace VeterinaryApi.Infrastructure.Services.Notifications;

internal class EmailOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
