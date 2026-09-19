using AssistantLogic.Model;
using AssistantLogic.Triggers.Weather;

namespace Assistant.Api.Contracts
{
    /// <summary>
    /// The weather actually used to evaluate triggers for this plan.
    /// </summary>
    public class WeatherDto
    {
        public int TemperatureC { get; set; }

        public WeatherType WeatherType { get; set; }

        public ScaleWind Wind { get; set; }

        public RainLevel RainLevel { get; set; }

        public SnowLevel SnowLevel { get; set; }

        public DateTimeOffset LastUpdate { get; set; }

        public static WeatherDto From(WeatherModel weather)
        {
            return new WeatherDto
            {
                TemperatureC = weather.Temperature,
                WeatherType = weather.WeatherType,
                Wind = weather.WindPower,
                RainLevel = weather.RainFall,
                SnowLevel = weather.SnowFall,
                LastUpdate = DateTime.SpecifyKind(weather.LastUpdate, DateTimeKind.Local)
            };
        }
    }
}
