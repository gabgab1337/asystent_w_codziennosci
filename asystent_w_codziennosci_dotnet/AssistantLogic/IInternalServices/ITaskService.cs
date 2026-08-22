using AssistantLogic.Triggers.TriggersTime;
using AssistantLogic.ViewModel;

namespace AssistantLogic.IInternalServices
{
    public interface ITaskService
    {
        ListTaskBaseInformationVM ListOfTaskBaseInfoVMForAsdPerson(int asdPersonID, string asdPersonName, TriggerWeekDayType? weekDay);

        TaskDetailsVM GetTaskDetailsVM(int id, int caregiverId);

        void Add(TaskAddEditVM viewModel, int caregiverId, int adsPersonID);

        void Update(TaskAddEditVM model);

        void Delete(int id);

        void Up(int asdPersonId,int taskId, TriggerWeekDayType? weekDay);
        void Down(int asdPersonId, int taskId, TriggerWeekDayType? weekDay);
        TaskAddEditVM GetTaskAddEditVM(int id);
        void RemoveTrigger(int taskId, int triggerId);

        public void AddTrigger(int taskId, int triggerId, bool negation);
    }
}
