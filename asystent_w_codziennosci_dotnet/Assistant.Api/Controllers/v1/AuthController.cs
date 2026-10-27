using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
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
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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

            return Ok(CreateLoginResponse(user));
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public ActionResult<LoginResponse> Refresh(RefreshTokenRequest request)
        {
            if (!accessTokenService.TryValidateRefresh(
                    request.AccessToken,
                    request.RefreshToken,
                    out int userId))
            {
                return Unauthorized(new ErrorResponse(INVALID_REFRESH_TOKEN));
            }

            UserDM? userEntity = userRepository.GetById(userId);
            if (userEntity == null)
            {
                return Unauthorized(new ErrorResponse(INVALID_REFRESH_TOKEN));
            }

            return Ok(CreateLoginResponse(ToSessionModel(userEntity)));
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

        private UserSessionModel ToSessionModel(UserDM user)
        {
            int? selectedProtegeId = null;
            string? selectedProtegeName = null;

            if (user.Type == UserType.caregiver && user.SelectedASD.HasValue)
            {
                UserDM? protege = userRepository.GetProtegeById(user.SelectedASD);
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
