using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;
using AssistantLogic.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Assistant.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private const string INVALID_CREDENTIALS = "Błędne dane logowania";
        private const string INVALID_REFRESH_TOKEN = "Nieprawidłowy token odświeżania";

        private readonly IUserService userService;
        private readonly IAccessTokenService accessTokenService;
        private readonly IUserRepository userRepository;

        public AuthController(
            IUserService userService,
            IAccessTokenService accessTokenService,
            IUserRepository userRepository)
        {
            this.userService = userService;
            this.accessTokenService = accessTokenService;
            this.userRepository = userRepository;
        }

        [AllowAnonymous]
        [EnableRateLimiting("login-policy")]
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            StatusCodes.Status400BadRequest,
            "application/problem+json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status429TooManyRequests,
            "application/problem+json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status500InternalServerError,
            "application/problem+json")]
        public async Task<IActionResult> LogIn(LoginRequest request, CancellationToken cancellationToken)
        {
            UserSessionModel? user = await userService.LogIn(new UserLoginVM
            {
                Username = request.Username,
                Password = request.Password
            }, cancellationToken);

            if (user == null)
            {
                return Unauthorized(new ErrorResponse(INVALID_CREDENTIALS));
            }

            return Ok(CreateLoginResponse(user));
        }

        [AllowAnonymous]
        [EnableRateLimiting("login-policy")]
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            StatusCodes.Status400BadRequest,
            "application/problem+json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status429TooManyRequests,
            "application/problem+json")]
        public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            if (!accessTokenService.TryValidateRefresh(
                    request.AccessToken,
                    request.RefreshToken,
                    out int userId))
            {
                return Unauthorized(new ErrorResponse(INVALID_REFRESH_TOKEN));
            }

            UserDM? userEntity = await userRepository.GetByIdAsync(userId, cancellationToken);
            if (userEntity == null)
            {
                return Unauthorized(new ErrorResponse(INVALID_REFRESH_TOKEN));
            }

            return Ok(CreateLoginResponse(await ToSessionModel(userEntity, cancellationToken)));
        }

        private LoginResponse CreateLoginResponse(UserSessionModel user)
        {
            AccessToken token = accessTokenService.Create(user);
            return new LoginResponse
            {
                AccessToken = token.Value,
                RefreshToken = token.RefreshToken,
                ExpiresAtUtc = token.ExpiresAtUtc,
                RefreshTokenExpiresAtUtc = token.RefreshTokenExpiresAtUtc,
                User = UserDto.FromSessionModel(user)
            };
        }

        private async Task<UserSessionModel> ToSessionModel(UserDM user, CancellationToken cancellationToken)
        {
            int? selectedProtegeId = null;
            string? selectedProtegeName = null;

            if (user.Type == UserType.caregiver && user.SelectedASD.HasValue)
            {
                UserDM? protege = await userRepository.GetByIdAsync(user.SelectedASD.Value, cancellationToken);
                if (protege?.Type == UserType.asdPerson
                    && protege.CaregiverId == user.Id)
                {
                    selectedProtegeId = protege.Id;
                    selectedProtegeName = protege.Username;
                }
            }

            return new UserSessionModel
            {
                Id = user.Id,
                UserName = user.Username,
                Type = user.Type,
                SelectedProtageId = selectedProtegeId,
                SelectedProtageName = selectedProtegeName,
                ColorPalete = user.ColorsPalete ?? "White",
                CurrentTaskId = user.CurrentTaskId
            };
        }
    }
}
