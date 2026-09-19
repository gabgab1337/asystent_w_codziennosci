using AssistantDatabase.Model;
using AssistantLogic.Model;

namespace Assistant.Api.Contracts
{
    /// <summary>
    /// Shape reused by the future <c>GET /api/v1/me</c>, so a client can skip that call after login.
    /// </summary>
    public class UserDto
    {
        private const string DEFAULT_COLOR_PALETTE = "White";

        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public UserType Type { get; set; }

        public int? SelectedProtegeId { get; set; }

        public string? SelectedProtegeName { get; set; }

        public string ColorPalette { get; set; } = DEFAULT_COLOR_PALETTE;

        public static UserDto FromSessionModel(UserSessionModel user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.UserName,
                Type = user.Type,
                SelectedProtegeId = user.SelectedProtageId,
                SelectedProtegeName = user.SelectedProtageName,
                ColorPalette = user.ColorPalete
            };
        }

        /// <summary>
        /// Protege values are resolved by the caller, which mirrors how <see cref="UserSessionModel"/>
        /// was filled on login: a selected protege that no longer exists is reported as none.
        /// </summary>
        public static UserDto FromEntity(UserDM user, int? selectedProtegeId, string? selectedProtegeName)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Type = user.Type,
                SelectedProtegeId = selectedProtegeId,
                SelectedProtegeName = selectedProtegeName,
                ColorPalette = user.ColorsPalete ?? DEFAULT_COLOR_PALETTE
            };
        }
    }
}
