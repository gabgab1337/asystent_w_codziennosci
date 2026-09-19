using AssistantLogic.Model;

namespace Assistant.Api.Authentication
{
    public interface IAccessTokenService
    {
        AccessToken Create(UserSessionModel user);
    }

    public record AccessToken(string Value, DateTime ExpiresAtUtc);
}
