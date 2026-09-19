namespace Assistant.Api.Authentication
{
    /// <summary>
    /// Claim names as they appear in the token. Inbound claim mapping is disabled so the
    /// short names survive to <c>HttpContext.User</c> and stay stable for the KMP client.
    /// </summary>
    public static class JwtClaims
    {
        public const string Subject = "sub";

        public const string UniqueName = "unique_name";

        public const string Role = "role";
    }
}
