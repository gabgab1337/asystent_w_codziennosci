using System.ComponentModel;

namespace AssistantDatabase.Model
{
    public enum TriggerType
    {
        [Description("Wybierz")]
        None,
        [Description("Czas")]
        Time,
        [Description("Pogoda")]
        Weather,
        [Description("Zdarzenie losowe")]
        RandomEvent,
    }
}
