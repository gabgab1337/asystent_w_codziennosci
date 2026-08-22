using System.ComponentModel;

namespace AssistantLogic.Triggers.Weather
{
    public enum DrizzleLevel
    {
        [Description("Brak mżawki")]
        NoneDrizzle,
        [Description("Lekko intensywnie mżawka")]
        LightIntensityDrizzle,
        [Description("Mżawka")]
        Drizzle,
        [Description("Mocno intensywnie mżawka")]
        HeavyIntensityDrizzle,
        [Description("Lekko intensywnie mżawka z deszczem")]
        LightIntensityDrizzleRain,
        [Description("Mżawka z deszczem")]
        DrizzleRain,
        [Description("Mocno intensywnie mżawka z deszczem")]
        HeavyIntensityDrizzleRain,
        [Description("Opady deszczu z mżawką")]
        ShowerRainAndDrizzle,
        [Description("Silne opady deszczu z mżawką")]
        HeavyShowerRainAndDrizzle,
        [Description("Opady mżawki")]
        ShowerDrizzle,
    }
}
