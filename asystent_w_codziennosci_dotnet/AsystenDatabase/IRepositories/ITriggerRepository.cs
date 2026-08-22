using AssistantDatabase.Model;

namespace AssistantDatabase.IRepositories
{
    public interface ITriggerRepository
    {
        void Add(TriggerDM trigger, int caregiverId);
        void Update(TriggerDM trigger);
        void Delete(int id);
        bool AllowDeleteTrigger(int id);
        TriggerDM GetById(int id);
        List<TriggerDM> ReadAllForCaregiver(int caregiverId);
        List<TriggerDM> ReadAllForTask(int taskId);
        List<TriggerDM> ReadAllForTaskToAdd(int taskId, int caregiverId);
        List<TriggerDM> ReadAllForPoint(int pointId);
        List<TriggerDM> ReadAllForPointToAdd(int pointId, int caregiverId);
    }
}
