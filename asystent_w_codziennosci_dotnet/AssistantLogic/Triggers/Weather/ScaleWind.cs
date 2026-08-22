using System.ComponentModel;

namespace AssistantLogic.Triggers.Weather
{
    public enum ScaleWind
    {
        [Description("Cisza")]
        Calm,
        [Description("Powiew")]
        LightAir,
        [Description("Słaby wiatr")]
        LightBreeze,
        [Description("Łagodny wiatr")]
        GentleBreeze,
        [Description("Umiarkowany wiatr")]
        ModerateBreeze,
        [Description("Dość silny wiatr")]
        FreshBreeze,
        [Description("Silny wiatr")]
        StrongBreeze,
        [Description("Bardzo silny wiatr")]
        HighWind,
        [Description("Wicher")]
        Gale,
        [Description("Silny wicher")]
        StrongGale,
        [Description("Bardzo silny wicher")]
        WholeGale,
        [Description("Gwałtowny wicher")]
        ViolentGale,
        [Description("Huragan")]
        Hurricane
    }
}
