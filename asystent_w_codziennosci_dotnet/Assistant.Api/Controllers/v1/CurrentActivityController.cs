using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IExternalServices;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;
using AssistantLogic.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assistant.Api.Controllers.v1
{
    /// <summary>
    /// ASD runtime. The current task id is stored on the user row. Each read recomputes the
    /// activity, and <see cref="ICurrentActivityService"/> closes a task whose points are all done.
    /// </summary>
    [ApiController]
    [Authorize(Roles = Roles.AsdPerson)]
    [Route("api/v1/me")]
    public class CurrentActivityController : ControllerBase
    {
        private const string UNKNOWN_USER = "Token nie wskazuje na istniejącego użytkownika";
        private const string NOT_YOUR_POINT = "Punkt nie należy do tego użytkownika";
        private const string NOT_YOUR_TASK = "Zadanie nie należy do tego użytkownika";
        private const string WEATHER_UNAVAILABLE =
            "Nie udało się pobrać danych pogodowych, aktualna aktywność jest chwilowo niedostępna";

        private readonly ICurrentActivityService currentActivityService;
        private readonly IUserRepository userRepository;
        private readonly IWeatherService weatherService;
        private readonly ITimeService timeService;
        private readonly ILogger<CurrentActivityController> logger;

        public CurrentActivityController(
            ICurrentActivityService currentActivityService,
            IUserRepository userRepository,
            IWeatherService weatherService,
            ITimeService timeService,
            ILogger<CurrentActivityController> logger)
        {
            this.currentActivityService = currentActivityService;
            this.userRepository = userRepository;
            this.weatherService = weatherService;
            this.timeService = timeService;
            this.logger = logger;
        }

        [HttpGet("current-activity")]
        [ProducesResponseType(typeof(CurrentActivityResponse), StatusCodes.Status200OK)]
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
            StatusCodes.Status503ServiceUnavailable,
            "application/json")]
        public async Task<ActionResult<CurrentActivityResponse>> GetCurrent(CancellationToken cancellationToken)
        {
            Response.Headers.Append("Cache-Control", "no-store");

            UserDM? user = await FindCaller(cancellationToken);
            if (user == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            PlanDayParameters? parameters = await TryBuildParameters();
            if (parameters == null)
            {
                return WeatherUnavailable();
            }

            ActualActivityVM activity = await currentActivityService.CreateAndPersistActualActivityAsync(
                user.Id,
                parameters,
                DateTime.Today,
                cancellationToken);
            return Ok(CurrentActivityResponse.From(activity));
        }

        [HttpGet("current-activity/summary")]
        [ProducesResponseType(typeof(CurrentActivitySummaryResponse), StatusCodes.Status200OK)]
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
            StatusCodes.Status503ServiceUnavailable,
            "application/json")]
        public async Task<ActionResult<CurrentActivitySummaryResponse>> GetSummary(
            CancellationToken cancellationToken)
        {
            Response.Headers.Append("Cache-Control", "no-store");

            UserDM? user = await FindCaller(cancellationToken);
            if (user == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            PlanDayParameters? parameters = await TryBuildParameters();
            if (parameters == null)
            {
                return WeatherUnavailable();
            }

            ActualActivityVM activity = await currentActivityService.CreateAndPersistActualActivityAsync(
                user.Id,
                parameters,
                DateTime.Today,
                cancellationToken);
            int nowMinutes = timeService.ConvertToMinutes(DateTime.Now);
            return Ok(CurrentActivitySummaryResponse.From(activity, nowMinutes));
        }

        /// <summary>
        /// Marks the explicitly identified point done. The point write, optional task completion,
        /// and persisted-pointer update are one transaction and never fetch weather.
        /// </summary>
        [HttpPost("current-activity/complete-point")]
        [ProducesResponseType(typeof(ActivityCompletionResponse), StatusCodes.Status200OK)]
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
        public async Task<ActionResult<ActivityCompletionResponse>> CompletePoint(
            CompletePointRequest request,
            CancellationToken cancellationToken)
        {
            Response.Headers.Append("Cache-Control", "no-store");

            UserDM? user = await FindCaller(cancellationToken);
            if (user == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            ActivityCompletionResult result = await currentActivityService.CompletePointAsync(
                user.Id,
                request.PointId,
                cancellationToken);
            if (!result.Found)
            {
                return Forbidden(NOT_YOUR_POINT, "point_not_owned");
            }

            return Ok(ActivityCompletionResponse.From(result));
        }

        /// <summary>
        /// Marks the explicitly identified task done. The completion and persisted-pointer clear
        /// are one transaction and never fetch weather.
        /// </summary>
        [HttpPost("tasks/{taskId:int}/complete")]
        [ProducesResponseType(typeof(ActivityCompletionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden,
            "application/json")]
        public async Task<ActionResult<ActivityCompletionResponse>> CompleteTask(
            int taskId,
            CancellationToken cancellationToken)
        {
            Response.Headers.Append("Cache-Control", "no-store");

            UserDM? user = await FindCaller(cancellationToken);
            if (user == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            ActivityCompletionResult result = await currentActivityService.CompleteTaskAsync(
                user.Id,
                taskId,
                cancellationToken);
            if (!result.Found)
            {
                return Forbidden(NOT_YOUR_TASK, "task_not_owned");
            }

            return Ok(ActivityCompletionResponse.From(result));
        }

        private async Task<UserDM?> FindCaller(CancellationToken cancellationToken)
        {
            int? userId = User.GetUserId();
            if (userId == null)
            {
                return null;
            }

            UserDM? user = await userRepository.GetByIdAsync(userId.Value, cancellationToken);
            return user?.Type == UserType.asdPerson ? user : null;
        }

        private async Task<PlanDayParameters?> TryBuildParameters()
        {
            WeatherModel? liveWeather;
            try
            {
                liveWeather = await weatherService.GetCurrentWeather();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to read the current weather for the current activity.");
                return null;
            }

            if (liveWeather == null)
            {
                logger.LogError("Weather provider returned no data, cannot evaluate activity triggers.");
                return null;
            }

            return new PlanDayParameters
            {
                CurrentDateTime = DateTime.Now,
                Weather = liveWeather
            };
        }

        private ObjectResult Forbidden(string message, string code)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ErrorResponse(message) { Code = code });
        }

        private ObjectResult WeatherUnavailable()
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ErrorResponse(WEATHER_UNAVAILABLE));
        }

    }
}
