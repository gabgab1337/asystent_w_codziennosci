using System.Security.Claims;

namespace Assistant.Api.Authentication
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Reads the authenticated user id from the <c>sub</c> claim. Returns null when the claim
        /// is missing or not an integer, which the caller should treat as an unusable token.
        /// </summary>
        public static int? GetUserId(this ClaimsPrincipal principal)
        {
            string? value = principal.FindFirst(JwtClaims.Subject)?.Value;

            if (int.TryParse(value, out int id))
            {
                return id;
            }

            return null;
        }
    }
}
