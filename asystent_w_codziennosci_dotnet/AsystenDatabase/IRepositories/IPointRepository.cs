using AssistantDatabase.Model;

namespace AssistantDatabase.IRepositories
{
    public interface IPointRepository
    {
        TaskPointDM GetById(int id);
        List<TaskPointDM> ReadAllForTask(int taskId);
        List<TaskPointDM> ReadAllDonePointsForTask(int taskId);
        List<TaskPointDM> ReadAllToDoPointsForTask(int taskId);
        void Add(TaskPointDM point);
        void Update(TaskPointDM point);
        void Up(int taskId, int pointId);
        void Down(int taskId, int pointId);
        void Delete(int id);
        void AddTrigger(int pointId, int triggerId, bool negation);
        void RemoveTrigger(int pointId, int triggerId);
        void DonePoint(int pointId);

        

    }
}
