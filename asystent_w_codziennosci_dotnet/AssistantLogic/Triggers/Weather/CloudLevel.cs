using System.ComponentModel;

namespace AssistantLogic.Triggers.Weather
{
    public enum CloudLevel
    {
        [Description("Lekkie zachmurzenie")]
        FewClouds,
        [Description("Zachmurzenie")]
        ScatteredClouds,
        [Description("Mocne zachmurzenie")]
        BrokenClouds,
        [Description("Całkowite zachmurzenie")]
        OvercastClouds
        
    }
}
