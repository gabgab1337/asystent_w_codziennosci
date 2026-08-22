using AssistantLogic.Model;
using AssistantLogic.ViewModel;

namespace AssistantLogic.IInternalServices
{
    public interface IPlanDayService
    {
        List<PlanDayTaskVM> GeneratePlan(int asdPersonId, PlanDayParameters planDayParameters);
        PlanDayParameters UpdatePlanDayParameters(PlanDayParameters planDayParameters);
        PlanDayVM CreateModelPlanDay(PlanDayParameters parameters, int asdPersonId);
        PlanDayPreviewVM CreateModelPlanDayPreview(PlanDayParameters parameters, int asdPersonId, string asdPersonName);
    }
}
