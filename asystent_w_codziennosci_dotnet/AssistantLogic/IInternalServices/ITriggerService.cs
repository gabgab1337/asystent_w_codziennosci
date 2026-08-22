using AssistantDatabase.Model;
using AssistantLogic.Model;
using AssistantLogic.Triggers;
using AssistantLogic.Triggers.TriggersTime;
using AssistantLogic.ViewModel;

namespace AssistantLogic.IInternalServices
{
    public interface ITriggerService
    {
        List<string> ShortNameTriggersForPoint(int pointId);

        List<string> ShortNameTriggersForTask(int taskId);

        List<string> ToListOfTriggerShortName(List<TriggerDM> listDM);
        List<TriggerBaseInformationVM> ToListOfTriggerBaseInformationVM(List<TriggerDM> listDM);
        public ListTriggerBaseInformationVM ToListOfTriggerBaseInformationVMWithProtageName(int caregiverId, string ProtageName);
        List<TriggerBaseInformationVM> GetTriggersBaseInfoToAddForTask(int taskId, int caregiverId);

        List<TriggerBaseInformationVM> GetTriggersBaseInfoForTask(int taskId);

        bool WorkingForWeekDay(TriggerWeekDayType? weekDay, int taskId);

        List<ITrigger> GetTriggersForTask(int taskId);

        List<ITrigger> GetTriggersForPoint(int pointId);

        TriggerDM? ToTriggerDM(TriggerAddEditVM vm);

        void Add(TriggerDM triggerDM,int caregiverId);

        TriggerAddEditVM GetTriggerAddEditVM(int id);
        void Update(TriggerAddEditVM model);

        void Delete(int id);

        bool CheckTask(TaskDM task, PlanDayParameters parameters);
        List<TaskDM> CheckTasks(List<TaskDM> listOfTasks, PlanDayParameters parameters);
        List<TaskPointDM> CheckPoints(List<TaskPointDM> listOfPoint, PlanDayParameters parameters);
    }
}
