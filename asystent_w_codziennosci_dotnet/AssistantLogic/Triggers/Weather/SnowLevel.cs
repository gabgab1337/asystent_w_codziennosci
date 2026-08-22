using System.ComponentModel;

namespace AssistantLogic.Triggers.Weather
{
    public enum SnowLevel
    {
        [Description("Brak śniegu")]
        NoneSnow,
        [Description("Lekki śnieg")]
        LightSnow,
        [Description("Śnieg")]
        Snow,
        [Description("Ciężki śnieg")]
        HeavySnow,
        [Description("Przelotny śnieg")]
        Sleet,
        [Description("Przelotne opady śniegu")]
        LightShowerSleet,
        [Description("Opady śniegu")]
        ShowerSleet,
        [Description("Lekki deszcz i śnieg")]
        LightRainAndSnow,
        [Description("Deszcz i śnieg")]
        RainAndSnow,
        [Description("Lekkie opady śniegu")]
        LightShowerSnow,
        [Description("Opady śniegu")]
        ShowerSnow,
        [Description("Silne opady śniegu")]
        HeavyShowerSnow
    }
}
