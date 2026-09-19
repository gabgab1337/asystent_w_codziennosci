namespace Assistant.Api.Authentication
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Symmetric HS256 signing key. Development only: it lives in appsettings.Development.json
        /// for the vertical slice and must move to a secret store before any shared environment.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        public int AccessTokenMinutes { get; set; } = 480;
    }
}
