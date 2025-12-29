namespace VeterinaryApi.Domain.Users
{
    public class UserSession
    {
        public Guid Id { get; private set; }

        public Guid UserId { get; set; }

        public string Token { get; set; } = null!;
        public UserSessionTokenType TokenType { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;

    }
    public enum UserSessionTokenType : byte
    {
        Refresh = 1
    }
}

