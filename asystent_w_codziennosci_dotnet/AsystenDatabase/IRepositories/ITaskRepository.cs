using AssistantDatabase.Model;

namespace AssistantDatabase.IRepositories
{
    public interface ITaskRepository
    {
        TaskDM Add(TaskDM task, int caregiverId, int adsPersonID);

        TaskDM GetById(int id);

        /// <summary>
        /// The task when it belongs to <paramref name="asdPersonId"/>. Missing and someone else's task are both null.
        /// </summary>
        TaskDM? ReadOwnedByAsdPerson(int taskId, int asdPersonId);
        TaskDM? GetByIdWithDetails(int id);
        Task<TaskDM?> GetOwnedByCaregiverAsync(
            int taskId,
            int caregiverId,
            CancellationToken cancellationToken = default);

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
        Task DeleteAsync(TaskDM task, CancellationToken cancellationToken = default);

        void DoneTask(int taskId);
        DateTime ReadLastDoneTaskTime(int taskId, DateTime currentDate);

    }
}
