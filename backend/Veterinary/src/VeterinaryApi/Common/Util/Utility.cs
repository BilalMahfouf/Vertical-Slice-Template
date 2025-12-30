namespace VeterinaryApi.Common.Util
{
    public static class Utility
    {
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
