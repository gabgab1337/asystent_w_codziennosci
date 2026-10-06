using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
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
        private const string NOT_YOUR_PROTEGE = "Podopieczny nie należy do tego opiekuna";

        private readonly IUserRepository userRepository;

        public UserController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
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

        [HttpPut("selected-protege")]
        [Authorize(Roles = Roles.Caregiver)]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public ActionResult<UserDto> SelectProtege(SelectProtegeRequest request)
        {
            int? caregiverId = User.GetUserId();
            if (caregiverId == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            UserDM? caregiver = userRepository.GetById(caregiverId.Value);
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            UserDM? protege = userRepository.GetById(request.ProtegeId);

            // A protege that does not exist and one owned by somebody else get the same answer,
            // so the endpoint cannot be used to probe which ids exist.
            bool ownedByCaller = protege != null
                && protege.Type == UserType.asdPerson
                && protege.CaregiverId == caregiverId;

            if (!ownedByCaller)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(NOT_YOUR_PROTEGE));
            }

            caregiver.SelectedASD = protege!.Id;
            userRepository.Update(caregiver);

            return Ok(UserDto.FromEntity(caregiver, protege.Id, protege.Username));
        }
    }
}
