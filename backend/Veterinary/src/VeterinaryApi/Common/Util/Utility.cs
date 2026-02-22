namespace VeterinaryApi.Common.Util
{
    /// <summary>General-purpose utility methods used across the application.</summary>
    public static class Utility
    {
        /// <summary>
        /// Builds a password-reset or email-verification callback URL by appending
        /// <paramref name="token"/> and <paramref name="email"/> as URL-encoded query parameters.
        /// </summary>
        internal static string GenerateResponseLink(string email, string token, string uri)
        {
            var param = new Dictionary<string, string>
                {
                    {"token",token},
                    {"email",email}
                };
            string link = $"{uri}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
            return link;
        }

    }
}
