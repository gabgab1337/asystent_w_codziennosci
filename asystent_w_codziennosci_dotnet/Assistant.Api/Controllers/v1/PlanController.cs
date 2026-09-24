using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using Assistant.Api.Middleware;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;
using AssistantLogic.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assistant.Api.Controllers.v1
{
    /// <summary>
    /// Thin adapter over <see cref="IPlanDayService"/>. Plan parameters come from the request and
    /// the server clock instead of the MVC session, so the endpoints are stateless.
    /// </summary>
    [ApiController]
    [Authorize]
    public class PlanController : ControllerBase
    {
        private const string UNKNOWN_USER = "Token nie wskazuje na istniejącego użytkownika";
        private const string NOT_YOUR_PROTEGE = "Podopieczny nie należy do tego opiekuna";
        private const string WEATHER_UNAVAILABLE =
            "Nie udało się pobrać danych pogodowych, plan dnia jest chwilowo niedostępny";

        private readonly IPlanDayService planDayService;
        private readonly IUserRepository userRepository;
        private readonly ILogger<PlanController> logger;

        public PlanController(
            IPlanDayService planDayService,
            IUserRepository userRepository,
            ILogger<PlanController> logger)
        {
            this.planDayService = planDayService;
            this.userRepository = userRepository;
            this.logger = logger;
        }

        [HttpGet("api/v1/me/plan")]
        [Authorize(Roles = Roles.AsdPerson)]
        [ProducesResponseType(typeof(PlanDayResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ExceptionHandleMiddleware), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
        public ActionResult<PlanDayResponse> GetMyPlan()
        {
            int? userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            UserDM? asdPerson = userRepository.GetById(userId.Value);
            if (asdPerson == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            PlanDayParameters? parameters = TryBuildParameters();
            if (parameters == null)
            {
                return WeatherUnavailable();
            }

            PlanDayVM plan = planDayService.CreateModelPlanDay(parameters, asdPerson.Id);

            return Ok(PlanDayResponse.From(asdPerson.Id, asdPerson.Username, parameters, plan.Tasks));
        }

        [HttpGet("api/v1/proteges/{protegeId:int}/plan")]
        [Authorize(Roles = Roles.Caregiver)]
        [ProducesResponseType(typeof(PlanDayResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ExceptionHandleMiddleware), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
        public ActionResult<PlanDayResponse> GetProtegePlan(int protegeId)
        {
            int? caregiverId = User.GetUserId();
            if (caregiverId == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            UserDM? protege = userRepository.GetById(protegeId);

            // A protege that does not exist and one owned by somebody else get the same answer,
            // so the endpoint cannot be used to probe which ids exist.
            bool ownedByCaller = protege != null
                && protege.Type == UserType.asdPerson
                && protege.CaregiverId == caregiverId;

            if (!ownedByCaller)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(NOT_YOUR_PROTEGE));
            }

            PlanDayParameters? parameters = TryBuildParameters();
            if (parameters == null)
            {
                return WeatherUnavailable();
            }

            PlanDayPreviewVM preview = planDayService.CreateModelPlanDayPreview(
                parameters,
                protege!.Id,
                protege.Username);

            return Ok(PlanDayResponse.From(protege.Id, protege.Username, parameters, preview.Tasks));
        }

        /// <summary>
        /// Builds the plan parameters for "now", including the weather fetch that the plan and
        /// trigger evaluation need. Returns null when the weather is unavailable, which is the
        /// one failure the legacy stack hits regularly.
        /// </summary>
        private PlanDayParameters? TryBuildParameters()
        {
            try
            {
                PlanDayParameters parameters = planDayService.UpdatePlanDayParameters(
                    new PlanDayParameters { CurrentDateTime = DateTime.Now });

                if (parameters.Weather == null)
                {
                    logger.LogError("Weather provider returned no data, cannot evaluate weather triggers.");
                    return null;
                }

                return parameters;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to read the current weather for the day plan.");
                return null;
            }
        }

        private ActionResult WeatherUnavailable()
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ErrorResponse(WEATHER_UNAVAILABLE));
        }
    }
}
