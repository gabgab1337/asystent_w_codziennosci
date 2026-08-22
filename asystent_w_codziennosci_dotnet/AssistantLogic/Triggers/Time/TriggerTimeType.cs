using System.ComponentModel;

namespace AssistantLogic.Triggers.TriggersTime
{
    public enum TriggerTimeType
    {
        [Description("Wybierz")]
        None,
        [Description("Wybrana data")]
        Data,
        [Description("Przedzial dat")]
        DateRange,
        [Description("Dzien tygodnia")]
        WeekDay,
        [Description("Odstep czasowy")]
        DateInterval,
        [Description("Dzień roboczy [pon - pt]")]
        WorkingDay,
        [Description("Weekend [sob - nd]")]
        Weekend
    }
}
