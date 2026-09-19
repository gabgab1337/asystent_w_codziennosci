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

            var claims = new List<Claim>
            {
                new Claim(JwtClaims.Subject, user.Id.ToString()),
                new Claim(JwtClaims.UniqueName, user.UserName),
                new Claim(JwtClaims.Role, user.Type.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: options.Issuer,
                audience: options.Audience,
                claims: claims,
                notBefore: issuedAt,
                expires: expiresAt,
                signingCredentials: credentials);

            return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
