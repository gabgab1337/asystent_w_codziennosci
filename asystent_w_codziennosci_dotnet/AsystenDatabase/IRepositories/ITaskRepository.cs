using AssistantDatabase.Model;

namespace AssistantDatabase.IRepositories
{
    public interface ITaskRepository
    {
        void Add(TaskDM task, int caregiverId, int adsPersonID);

        TaskDM GetById(int id);
        TaskDM? GetByIdWithDetails(int id);

        void Update(TaskDM task);

        public void AddTrigger(int taskId, int triggerId, bool negation);
        public void RemoveTrigger(int taskId, int triggerId);

        List<TaskDM> ReadAllForCaregiver(int caregiverId);
        List<TaskDM> ReadAllForAsdPerson(int asdPersonId);
        TaskDM? ReadLastDoneTaskForAsdPerson(int asdPersonId, DateTime currentDate);
        List<TaskDM> ReadAllDoneTasksForAsdPerson(int asdPersonId, DateTime currentDate);

        List<TaskDM> ReadAllToDoTasksForAsdPerson(int asdPersonId, DateTime currentDate);

        void Up(int asdPersonId, int taskId);

        bool UpOverNumber(int asdPersonId, int taskId, int number);

        void Down(int asdPersonId, int taskId);

        bool DownUnderNumber(int asdPersonId, int taskId, int number);

        void Delete(int id);

        void DoneTask(int taskId);
        DateTime ReadLastDoneTaskTime(int taskId, DateTime currentDate);

    }
}
