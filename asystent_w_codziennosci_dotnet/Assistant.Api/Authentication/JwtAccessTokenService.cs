using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AssistantLogic.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Assistant.Api.Authentication
{
    public class JwtAccessTokenService : IAccessTokenService
    {
        private readonly JwtOptions options;

        public JwtAccessTokenService(IOptions<JwtOptions> options)
        {
            this.options = options.Value;
        }

        public AccessToken Create(UserSessionModel user)
        {
            DateTime issuedAt = DateTime.UtcNow;
            DateTime expiresAt = issuedAt.AddMinutes(options.AccessTokenMinutes);

            var userClaims = new List<Claim>
            {
                new Claim(JwtClaims.Subject, user.Id.ToString()),
                new Claim(JwtClaims.UniqueName, user.UserName),
                new Claim(JwtClaims.Role, user.Type.ToString())
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
                SecurityAlgorithms.HmacSha256);

            string accessToken = CreateToken(
                userClaims.Append(new Claim(JwtClaims.TokenUse, JwtClaims.AccessTokenUse)),
                issuedAt,
                expiresAt,
                credentials);

            DateTime refreshTokenExpiresAt = issuedAt.AddDays(options.RefreshTokenDays);
            string refreshToken = CreateToken(
                userClaims.Append(new Claim(JwtClaims.TokenUse, JwtClaims.RefreshTokenUse)),
                issuedAt,
                refreshTokenExpiresAt,
                credentials);

            return new AccessToken(
                accessToken,
                expiresAt,
                refreshToken,
                refreshTokenExpiresAt);
        }

        public bool TryValidateRefresh(
            string accessToken,
            string refreshToken,
            out int userId)
        {
            userId = 0;

            try
            {
                ClaimsPrincipal accessPrincipal = ValidateToken(
                    accessToken,
                    validateLifetime: false);
                ClaimsPrincipal refreshPrincipal = ValidateToken(
                    refreshToken,
                    validateLifetime: true);

                if (!accessPrincipal.HasClaim(
                        JwtClaims.TokenUse,
                        JwtClaims.AccessTokenUse)
                    || !refreshPrincipal.HasClaim(
                        JwtClaims.TokenUse,
                        JwtClaims.RefreshTokenUse))
                {
                    return false;
                }

                string? accessSubject = accessPrincipal.FindFirst(JwtClaims.Subject)?.Value;
                string? refreshSubject = refreshPrincipal.FindFirst(JwtClaims.Subject)?.Value;
                string? accessUsername = accessPrincipal.FindFirst(JwtClaims.UniqueName)?.Value;
                string? refreshUsername = refreshPrincipal.FindFirst(JwtClaims.UniqueName)?.Value;
                string? accessRole = accessPrincipal.FindFirst(JwtClaims.Role)?.Value;
                string? refreshRole = refreshPrincipal.FindFirst(JwtClaims.Role)?.Value;

                return accessSubject == refreshSubject
                    && accessUsername == refreshUsername
                    && accessRole == refreshRole
                    && int.TryParse(accessSubject, out userId)
                    && userId > 0;
            }
            catch (Exception exception) when (
                exception is SecurityTokenException
                or ArgumentException)
            {
                return false;
            }
        }

        private string CreateToken(
            IEnumerable<Claim> claims,
            DateTime issuedAt,
            DateTime expiresAt,
            SigningCredentials credentials)
        {
            var token = new JwtSecurityToken(
                issuer: options.Issuer,
                audience: options.Audience,
                claims: claims.Append(
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())),
                notBefore: issuedAt,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ClaimsPrincipal ValidateToken(string token, bool validateLifetime)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,
                ValidateAudience = true,
                ValidAudience = options.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(options.Key)),
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = JwtClaims.UniqueName,
                RoleClaimType = JwtClaims.Role
            };

            return new JwtSecurityTokenHandler().ValidateToken(
                token,
                validationParameters,
                out _);
        }
    }
}
