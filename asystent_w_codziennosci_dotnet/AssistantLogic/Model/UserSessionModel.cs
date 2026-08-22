using AssistantDatabase.Model;
using AssistantLogic.Triggers.TriggersTime;

namespace AssistantLogic.Model
{
    public class UserSessionModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public UserType Type { get; set; }
        public int? SelectedProtageId { get; set; }
        public string? SelectedProtageName { get; set; }
        public PlanDayParameters planDayParameters { get; set; }
        public PlanDayParameters? planDayPreviewParameters { get; set; }
        public int? CurrentTaskId { get; set; } = null;
        public TriggerWeekDayType? WeekDayForTaskListCaragiver { get; set; }
        public string ColorPalete { get; set; } = "White";
    }
}
