using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assistant.Api.Controllers.v1
{
    /// <summary>
    /// Caregiver-owned ASD users. Identity comes from the token; nothing is stored in session.
    /// </summary>
    [ApiController]
    [Authorize(Roles = Roles.Caregiver)]
    [Route("api/v1/proteges")]
    public class ProtegesController : ControllerBase
    {
        private const string UNKNOWN_USER = "Token nie wskazuje na istniejącego użytkownika";
        private const string USERNAME_TAKEN = "Podana nazwa użytkownika '{0}' już istnieje";

        private readonly IUserRepository userRepository;

        public ProtegesController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ProtegeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public ActionResult<List<ProtegeDto>> Get()
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            List<ProtegeDto> proteges = userRepository
                .ReadAllForCaregiver(caregiver.Id)
                .Select(user => ProtegeDto.FromEntity(user, caregiver.SelectedASD))
                .ToList();

            return Ok(proteges);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProtegeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public ActionResult<ProtegeDto> Create(CreateProtegeRequest request)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            if (userRepository.ExistLogin(request.Username))
            {
                return Conflict(new ErrorResponse(string.Format(USERNAME_TAKEN, request.Username)));
            }

            // Same shape as UserService.CreateUserForCaregiver: an ASD user owned by this caregiver.
            // UserRepository.Add hashes the supplied password with the existing SHA-256 helper.
            UserDM protege = new UserDM
            {
                Username = request.Username,
                Password = request.Password,
                Type = UserType.asdPerson,
                CaregiverId = caregiver.Id
            };

            userRepository.Add(protege);

            ProtegeDto dto = ProtegeDto.FromEntity(protege, caregiver.SelectedASD);
            return CreatedAtAction(nameof(Get), dto);
        }

        /// <summary>
        /// The token's <c>sub</c> claim, confirmed against the database. Null when the token
        /// does not point at a user that still exists.
        /// </summary>
        private UserDM? FindCaller()
        {
            int? caregiverId = User.GetUserId();
            if (caregiverId == null)
            {
                return null;
            }

            return userRepository.GetById(caregiverId.Value);
        }
    }
}
