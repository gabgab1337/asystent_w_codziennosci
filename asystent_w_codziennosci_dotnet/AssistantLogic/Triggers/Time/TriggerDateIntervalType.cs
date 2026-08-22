using System.ComponentModel;

namespace AssistantLogic.Triggers.TriggersTime
{
    public enum TriggerDateIntervalType
    {
        [Description("Wybierz")]
        None,
        [Description("codziennie")]
        EveryDay,
        [Description("co 2 dni")]
        EveryTwoDays,
        [Description("co 3 dni")]
        EveryThreeDays,
        [Description("co 2 tygodnie")]
        EveryTwoWeeks,
        [Description("co 3 tygodnie")]
        EveryThreeWeeks,
        [Description("co miesiąc")]
        EveryMonth,
        [Description("co 2 miesiące")]
        EveryTwoMonths,
        [Description("co 3 miesiące")]
        EveryThreeMonths,
        [Description("co rok")]
        EveryYear,
        [Description("co 2 lata")]
        Every2Years,
        [Description("co wielki czwartek")]
        EveryMaundyThursday,
        [Description("co wielki piątek")]
        EveryGoodFriday,
        [Description("co wielką sobotę")]
        EveryHolySaturday,
        [Description("co wielkanoc")]
        EveryEasterDay,
        [Description("co poniedziałek wielkanocny")]
        EveryEasterMonday,
    }
}
