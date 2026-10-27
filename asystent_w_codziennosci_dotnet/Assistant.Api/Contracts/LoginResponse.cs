namespace Assistant.Api.Contracts
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public string TokenType { get; set; } = "Bearer";

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime RefreshTokenExpiresAtUtc { get; set; }

        public UserDto User { get; set; } = new UserDto();
    }
}
