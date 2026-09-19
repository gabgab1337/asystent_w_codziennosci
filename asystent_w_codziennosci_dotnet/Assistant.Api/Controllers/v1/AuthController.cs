using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;
using AssistantLogic.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assistant.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private const string INVALID_CREDENTIALS = "Błędne dane logowania";

        private readonly IUserService userService;
        private readonly IAccessTokenService accessTokenService;

        public AuthController(IUserService userService, IAccessTokenService accessTokenService)
        {
            this.userService = userService;
            this.accessTokenService = accessTokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public ActionResult<LoginResponse> LogIn(LoginRequest request)
        {
            UserSessionModel? user = userService.LogIn(new UserLoginVM
            {
                Username = request.Username,
                Password = request.Password
            });

            if (user == null)
            {
                return Unauthorized(new ErrorResponse(INVALID_CREDENTIALS));
            }

            AccessToken token = accessTokenService.Create(user);

            return Ok(new LoginResponse
            {
                AccessToken = token.Value,
                ExpiresAtUtc = token.ExpiresAtUtc,
                User = UserDto.FromSessionModel(user)
            });
        }
    }
}
