using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using AssistantLogic.IExternalServices;
using AssistantLogic.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assistant.Api.Controllers.v1
{
    [ApiController]
    [Authorize(Roles = Roles.Caregiver + "," + Roles.AsdPerson)]
    public class WeatherController : ControllerBase
    {
        private const string WEATHER_UNAVAILABLE = "Nie udało się pobrać danych pogodowych";

        private readonly IWeatherService weatherService;
        private readonly ILogger<WeatherController> logger;

        public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
        {
            this.weatherService = weatherService;
            this.logger = logger;
        }

        [HttpGet("api/v1/weather/current")]
        [ProducesResponseType(typeof(WeatherDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<WeatherDto>> GetCurrent()
        {
            try
            {
                WeatherModel? weather = await weatherService.GetCurrentWeather();
                if (weather == null)
                {
                    logger.LogError("Weather provider returned no data.");
                    return WeatherUnavailable();
                }

                return Ok(WeatherDto.From(weather));
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to read the current weather.");
                return WeatherUnavailable();
            }
        }

        private ActionResult WeatherUnavailable()
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ErrorResponse(WEATHER_UNAVAILABLE));
        }
    }
}
