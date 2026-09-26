using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using Assistant.Api.Middleware;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assistant.Api.Controllers.v1
{
    /// <summary>
    /// Replaces the MVC session user: identity comes from the token, the rest from the database.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/me")]
    public class UserController : ControllerBase
    {
        private const string UNKNOWN_USER = "Token nie wskazuje na istniejącego użytkownika";

        private readonly IUserRepository userRepository;

        public UserController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ExceptionHandleMiddleware), StatusCodes.Status500InternalServerError)]
        public ActionResult<UserDto> Get()
        {
            int? userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            UserDM? user = userRepository.GetById(userId.Value);
            if (user == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            int? selectedProtegeId = null;
            string? selectedProtegeName = null;

            if (user.Type == UserType.caregiver && user.SelectedASD.HasValue)
            {
                UserDM? protege = userRepository.GetProtegeById(user.SelectedASD);
                if (protege != null)
                {
                    selectedProtegeId = protege.Id;
                    selectedProtegeName = protege.Username;
                }
            }

            return Ok(UserDto.FromEntity(user, selectedProtegeId, selectedProtegeName));
        }

        [HttpGet("throw-test")]
        public IActionResult ThrowTest()
        {
            throw new InvalidOperationException("Test middleware wyjątków");
        }
    }
}
