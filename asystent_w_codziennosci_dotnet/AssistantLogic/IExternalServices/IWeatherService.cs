using AssistantLogic.Model;

namespace AssistantLogic.IExternalServices
{
    public interface IWeatherService
    {
        Task<WeatherModel?> GetCurrentWeather();
    }
}
