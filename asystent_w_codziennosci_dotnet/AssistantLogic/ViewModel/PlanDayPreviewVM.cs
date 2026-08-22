using AssistantLogic.Model;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssistantLogic.ViewModel
{
    public class PlanDayPreviewVM
    {
        public string actualProtegeName { get; set; }
        public List<PlanDayTaskVM> Tasks { get; set; }
        public PlanDayParameters Parameters { get; set; }
        public PlanDayParameters ActualParameters { get; set; }
        public List<SelectListItem> WindLevels { get; set; }
        public List<SelectListItem> WeatherTypes { get; set; }
        public List<SelectListItem> RainLevels { get; set; }
        public List<SelectListItem> SnowLevels { get; set; }

    }
}
