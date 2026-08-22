using AssistantLogic.Model;
using AssistantLogic.ViewModel;

namespace AssistantLogic.IInternalServices
{
    public interface ICurrentActivityService
    {
        void DonePoint(int taskId, int? pointId);

        ActualActivityVM CreateActualActivity(int asdPersonId, int? currentTaskId, PlanDayParameters parameters, DateTime currentDate);
    }
}
