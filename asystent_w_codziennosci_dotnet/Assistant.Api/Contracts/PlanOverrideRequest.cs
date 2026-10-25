using WeatherKind = AssistantLogic.Model.WeatherType;
using RainAmount = AssistantLogic.Triggers.Weather.RainLevel;
using WindSpeed = AssistantLogic.Triggers.Weather.ScaleWind;
using SnowAmount = AssistantLogic.Triggers.Weather.SnowLevel;
using AssistantLogic.Model;

namespace Assistant.Api.Contracts
{
    /// <summary>
    /// Optional clock and weather for a caregiver plan preview.
    /// Omit <see cref="Weather"/> to use the live forecast, and omit <see cref="At"/> to use the server clock.
    /// </summary>
    public class PlanOverrideRequest
    {
        public DateTimeOffset? At { get; set; }

        public WeatherOverrideDto? Weather { get; set; }
    }

    /// <summary>
    /// Weather values substituted for the live forecast. Each omitted field keeps its live value.
    /// </summary>
    public class WeatherOverrideDto
    {
        public int? TemperatureC { get; set; }

        public WeatherKind? WeatherType { get; set; }

        public RainAmount? RainLevel { get; set; }

        public WindSpeed? Wind { get; set; }

        public SnowAmount? SnowLevel { get; set; }

        public WeatherModel ApplyTo(WeatherModel liveWeather)
        {
            return new WeatherModel
            {
                Temperature = TemperatureC ?? liveWeather.Temperature,
                WeatherType = WeatherType ?? liveWeather.WeatherType,
                RainFall = RainLevel ?? liveWeather.RainFall,
                WindPower = Wind ?? liveWeather.WindPower,
                SnowFall = SnowLevel ?? liveWeather.SnowFall,
                Cloud = liveWeather.Cloud,
                ThunderstormLevel = liveWeather.ThunderstormLevel,
                DrizzleLevel = liveWeather.DrizzleLevel,
                LastUpdate = liveWeather.LastUpdate
            };
        }
    }
}
