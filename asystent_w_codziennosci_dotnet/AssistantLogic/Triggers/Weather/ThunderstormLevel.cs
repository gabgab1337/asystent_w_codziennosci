using System.ComponentModel;

namespace AssistantLogic.Triggers.Weather
{
    public enum ThunderstormLevel
    {
        [Description("Brak burzy")]
        NoneThunderstorm,
        [Description("Burza z lekkim deszczem")]
        ThunderstormWithLightRain,
        [Description("Burza z deszczem")]
        ThunderstormWithRain,
        [Description("Burza z mocnym deszczem")]
        ThunderstormWithHeavyRain,
        [Description("Lekka burza")]
        LightThunderstorm,
        [Description("Burza")]
        Thunderstorm,
        [Description("Mocna burza")]
        HeavyThunderstorm,
        [Description("Szarpana burza")]
        RaggedThunderstorm,
        [Description("Burza z lekką mżawką")]
        ThunderstormWithLightDrizzle,
        [Description("Burza z mżawką")]
        ThunderstormWithDrizzle,
        [Description("Burza z mocną mżawką")]
        ThunderstormWithHeavyDrizzle
    }
}
