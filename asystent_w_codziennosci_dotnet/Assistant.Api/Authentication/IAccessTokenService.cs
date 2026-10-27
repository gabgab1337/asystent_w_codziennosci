using AssistantLogic.Model;

namespace Assistant.Api.Authentication
{
    public interface IAccessTokenService
    {
        AccessToken Create(UserSessionModel user);

        bool TryValidateRefresh(
            string accessToken,
            string refreshToken,
            out int userId);
    }

    public record AccessToken(
        string Value,
        DateTime ExpiresAtUtc,
        string RefreshToken = "",
        DateTime RefreshTokenExpiresAtUtc = default);
}
