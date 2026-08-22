using AssistantLogic.Triggers.TriggersTime;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssistantLogic.ViewModel
{
    public class ListTaskBaseInformationVM
    {
        public string actualProtegeName { get; set; }
        public List<TaskBaseInformationVM> Tasks { get; set; } = new List<TaskBaseInformationVM>();
        public TriggerWeekDayType? WeekDayType { get; set; }
        public List<SelectListItem> WeekDayTypes { get; set; } = new List<SelectListItem>();
    }
}
