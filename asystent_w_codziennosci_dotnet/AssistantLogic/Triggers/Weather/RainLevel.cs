using System.ComponentModel;

namespace AssistantLogic.Triggers.Weather
{
    public enum RainLevel
    {
        [Description("Brak deszczu")]
        NoneRain,
        [Description("Lekki deszcz")]
        LightRain,
        [Description("Umiarkowany deszcz")]
        ModerateRain,
        [Description("Ulewny deszcz o dużej intensywności")]
        HeavyIntensityRain,
        [Description("Bardzo ulewny deszcz")]
        VeryHeavyRain,
        [Description("Ekstremalny deszcz")]
        ExtremeRain,
        [Description("Marznący deszcz")]
        FreezingRain,
        [Description("Lekko intensywne opady deszczu")]
        LightIntensityShowerRain,
        [Description("Intensywne opady deszczu")]
        ShowerRain,
        [Description("Mocno intensywne opady deszczu")]
        HightIntensityShowerRain,
        [Description("Poszarpany deszcz")]
        RaggedShowerRain
    }
}
