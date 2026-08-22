using AssistantLogic.Triggers.Weather;

namespace AssistantLogic.Model
{
    public class WeatherModel
    {
        public int Temperature { get; set; }
        public ScaleWind WindPower { get; set; }

        public WeatherType WeatherType { get; set; }
        public CloudLevel Cloud { get; set; }
        public ThunderstormLevel ThunderstormLevel { get; set; }
        public DrizzleLevel DrizzleLevel { get; set; }
        public RainLevel RainFall { get; set; }

        public SnowLevel SnowFall { get; set; }

        public DateTime LastUpdate { get; set; }

    }
}
