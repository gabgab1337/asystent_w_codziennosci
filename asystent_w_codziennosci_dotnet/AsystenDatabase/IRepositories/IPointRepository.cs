using AssistantDatabase.Model;

namespace AssistantDatabase.IRepositories
{
    public interface IPointRepository
    {
        TaskPointDM GetById(int id);
        Task<TaskPointDM?> GetOwnedByCaregiverAsync(
            int pointId,
            int caregiverId,
            CancellationToken cancellationToken = default);
        Task<bool> IsPhotoOwnedByCaregiverAsync(
            string photoPath,
            int caregiverId,
            CancellationToken cancellationToken = default);
        Task<bool> IsPhotoOwnedByAsdPersonAsync(
            string photoPath,
            int asdPersonId,
            CancellationToken cancellationToken = default);
        List<TaskPointDM> ReadAllForTask(int taskId);
        Task<List<string>> ReadPhotoPathsForTaskAsync(
            int taskId,
            CancellationToken cancellationToken = default);
        Task<List<string>> ReadPhotoPathsForAsdPersonAsync(
            int asdPersonId,
            CancellationToken cancellationToken = default);
        List<TaskPointDM> ReadAllDonePointsForTask(int taskId);
        List<TaskPointDM> ReadAllToDoPointsForTask(int taskId);
        TaskPointDM Add(TaskPointDM point);
        void Update(TaskPointDM point);
        Task UpdateAsync(TaskPointDM point, CancellationToken cancellationToken = default);
        void Up(int taskId, int pointId);
        void Down(int taskId, int pointId);
        void Delete(int id);
        Task DeleteAsync(TaskPointDM point, CancellationToken cancellationToken = default);
        void AddTrigger(int pointId, int triggerId, bool negation);
        void RemoveTrigger(int pointId, int triggerId);
        void DonePoint(int pointId);

        

    }
}
