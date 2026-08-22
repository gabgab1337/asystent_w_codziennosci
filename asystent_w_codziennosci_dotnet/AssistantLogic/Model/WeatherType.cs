using System.ComponentModel;

namespace AssistantLogic.Model
{
    public enum WeatherType
    {
        [Description("Dowolna")]
        Default,
        [Description("Burza")]
        Thunderstorm,
        [Description("Mżawka")]
        Drizzle,
        [Description("Deszcz")]
        Rain,
        [Description("Śnieg")]
        Snow,
        [Description("Dym")]
        Smoke,
        [Description("Zamglenie")]
        Haze,
        [Description("Pył")]
        Dust,
        [Description("Mgła")]
        Fog,
        [Description("Piasek")]
        Sand,
        [Description("Popiół")]
        Ash,
        [Description("Szkwał")]
        Squall,
        [Description("Tornado")]
        Tornado,
        [Description("Czysto")]
        Clear,
        [Description("Chmury")]
        Clouds
    }
}
