using AssistantDatabase.Model;

namespace AssistantLogic.IInternalServices
{
    public interface ITimeService
    {
        int GetTimeBegin(TaskDM task, TaskDM? taskBefore);
        int GetTimeBegin(TaskDM task, TaskDM? taskBefore, DateTime currentDateTime, DateTime currentDate);
        int GetTimeEnd(TaskDM task, List<TaskDM> listToDoTaskDM, int timeBegin);
        double CalculatePecentSpeed(TaskDM task, List<TaskDM> listToDoTaskDM, int timeBegin);
        string ConvertToTimeHHMM(int allMinutes);
        int ConvertToMinutes(DateTime time);
        string ConvertToTimeHourMinutes(int allMinutes);
       

    }
}
