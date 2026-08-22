using AssistantDatabase.Model;
using AssistantLogic.Triggers.Time;
using AssistantLogic.Triggers.Weather;
using AssistantLogic.ViewModel;

namespace AssistantLogic.Triggers
{
    public class TriggerCreator
    {
        public static ITrigger CreateTrigger(TriggerDM dm)
        {
            switch (dm.Type)
            {
                case TriggerType.Time:
                    TriggerTime trigger = new TriggerTime(dm);
                    return trigger;
                case TriggerType.Weather:
                    TriggerWeather triggerWeather = new TriggerWeather(dm);
                    return triggerWeather;
                case TriggerType.RandomEvent:
                    return null;
            }
            return null;
        }

        public static TriggerDM ConvertToDM(TriggerAddEditVM vm)
        {
            switch (vm.Type)
            {
                case TriggerType.Time:
                    return TriggerTime.ConvertToDM(vm);
                case TriggerType.Weather:
                    return TriggerWeather.ConvertToDM(vm);
                case TriggerType.RandomEvent:
                    return null;
            }
            return null;
        }
    }
}
