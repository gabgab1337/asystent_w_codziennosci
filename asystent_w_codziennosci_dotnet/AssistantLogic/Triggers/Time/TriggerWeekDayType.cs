using System.ComponentModel;

namespace AssistantLogic.Triggers.TriggersTime
{
    public enum TriggerWeekDayType
    {
        [Description("Wybierz")]
        None,
        [Description("Poniedziałek")]
        Monday,
        [Description("Wtorek")]
        Tuesday,
        [Description("Środa")]
        Wednesday,
        [Description("Czwartek")]
        Thursday,
        [Description("Piątek")]
        Friday,
        [Description("Sobota")]
        Saturday,
        [Description("Niedziela")]
        Sunday
    }
}
