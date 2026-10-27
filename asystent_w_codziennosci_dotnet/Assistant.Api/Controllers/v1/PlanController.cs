using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IExternalServices;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;
using AssistantLogic.Services;
using AssistantLogic.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Numerics;

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
        private readonly IWeatherService weatherService;
        private readonly ILogger<PlanController> logger;
        private readonly IWeatherService weatherService;

        public PlanController(
            IPlanDayService planDayService,
            IUserRepository userRepository,
            IWeatherService weatherService,
            ILogger<PlanController> logger)
        {
            this.planDayService = planDayService;
            this.userRepository = userRepository;
            this.weatherService = weatherService;
            this.logger = logger;
        }
        /*
        [HttpGet("api/v1/weather-cache-test")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetWeather()
        {
            long start1 = Stopwatch.GetTimestamp();
            WeatherModel weather = await weatherService.GetCurrentWeather();
            TimeSpan elapsedTime1 = Stopwatch.GetElapsedTime(start1);
            long start2 = Stopwatch.GetTimestamp();
            await weatherService.GetCurrentWeather();
            TimeSpan elapsedTime2 = Stopwatch.GetElapsedTime(start2);
            return Ok(new { weather, elapsedTime1, elapsedTime2 });
        }*/

        [HttpGet("api/v1/me/plan")]
        [Authorize(Roles = Roles.AsdPerson)]
        [ProducesResponseType(typeof(PlanDayResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            StatusCodes.Status400BadRequest,
            "application/problem+json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status500InternalServerError,
            "application/problem+json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status503ServiceUnavailable,
            "application/json")]
        public async Task<ActionResult<PlanDayResponse>> GetMyPlan(
            [FromQuery] DateTimeOffset? at,
            CancellationToken cancellationToken)
        {
            Response.Headers.Append("Cache-Control", "no-store");

            int? userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            UserDM? asdPerson = await userRepository.GetByIdAsync(userId.Value, cancellationToken);
            if (asdPerson == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            PlanDayParameters? parameters = await TryBuildParameters(at, weatherOverride: null);
            if (parameters == null)
            {
                return WeatherUnavailable();
            }

            // CreateModelPlanDay always rewrites CurrentDateTime to DateTime.Now, which would
            // drop ?at=. GeneratePlan keeps the parameters built above.
            List<PlanDayTaskVM> tasks = planDayService.GeneratePlan(asdPerson.Id, parameters);

            return Ok(PlanDayResponse.From(asdPerson.Id, asdPerson.Username, parameters, tasks, at));
        }

        /// <summary>
        /// Caregiver preview. Optional values in the JSON body override the server clock and
        /// corresponding live weather fields.
        /// </summary>
        [HttpPost("api/v1/proteges/{protegeId:int}/plan")]
        [Authorize(Roles = Roles.Caregiver)]
        [ProducesResponseType(typeof(PlanDayResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            StatusCodes.Status400BadRequest,
            "application/problem+json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status500InternalServerError,
            "application/problem+json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status503ServiceUnavailable,
            "application/json")]
        public async Task<ActionResult<PlanDayResponse>> CreateProtegePlanPreview(
            int protegeId,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] PlanOverrideRequest? request,
            CancellationToken cancellationToken)
        {
            Response.Headers.Append("Cache-Control", "no-store");

            int? caregiverId = User.GetUserId();
            if (caregiverId == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            UserDM? protege = await userRepository.GetByIdAsync(protegeId, cancellationToken);

            // A protege that does not exist and one owned by somebody else get the same answer,
            // so the endpoint cannot be used to probe which ids exist.
            bool ownedByCaller = protege != null
                && protege.Type == UserType.asdPerson
                && protege.CaregiverId == caregiverId;

            if (!ownedByCaller)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(NOT_YOUR_PROTEGE));
            }

            PlanDayParameters? parameters = await TryBuildParameters(request?.At, request?.Weather);
            if (parameters == null)
            {
                return WeatherUnavailable();
            }

            PlanDayPreviewVM preview = planDayService.CreateModelPlanDayPreview(
                parameters,
                protege!.Id,
                protege.Username);

            return Ok(PlanDayResponse.From(
                protege.Id,
                protege.Username,
                parameters,
                preview.Tasks,
                request?.At));
        }

        /// <summary>
        /// Builds plan parameters from an optional instant and the live weather. A partial weather
        /// override is merged onto the live reading. Returns null when that reading is unavailable.
        /// </summary>
        private async Task<PlanDayParameters?> TryBuildParameters(
            DateTimeOffset? at,
            WeatherOverrideDto? weatherOverride)
        {
            WeatherModel? liveWeather;
            try
            {
                liveWeather = await weatherService.GetCurrentWeather();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to read the current weather for the day plan.");
                return null;
            }

            if (liveWeather == null)
            {
                logger.LogError("Weather provider returned no data, cannot evaluate weather triggers.");
                return null;
            }

            // DateTimeOffset.DateTime deliberately keeps the supplied wall-clock components and
            // marks them Unspecified. Trigger evaluation uses local clock fields, so an input such
            // as 08:30+02:00 remains 08:30 instead of being shifted to the server time zone.
            return new PlanDayParameters
            {
                CurrentDateTime = at?.DateTime ?? DateTime.Now,
                Weather = weatherOverride?.ApplyTo(liveWeather) ?? liveWeather
            };
        }

        private ActionResult WeatherUnavailable()
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ErrorResponse(WEATHER_UNAVAILABLE));
        }
    }
}
