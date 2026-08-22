using AssistantLogic.Model;

namespace AssistantLogic.ViewModel
{
    public class PlanDayVM
    {
        public List<PlanDayTaskVM> Tasks { get; set; }
        public PlanDayParameters Parameters { get; set; }
    }
}
