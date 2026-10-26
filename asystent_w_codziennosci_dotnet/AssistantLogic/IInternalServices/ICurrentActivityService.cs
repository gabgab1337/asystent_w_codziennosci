using AssistantLogic.Model;
using AssistantLogic.ViewModel;

namespace AssistantLogic.IInternalServices
{
    public interface ICurrentActivityService
    {
        Task<ActivityCompletionResult> CompletePointAsync(
            int asdPersonId,
            int pointId,
            CancellationToken cancellationToken = default);

        Task<ActivityCompletionResult> CompleteTaskAsync(
            int asdPersonId,
            int taskId,
            CancellationToken cancellationToken = default);

        Task<ActualActivityVM> CreateAndPersistActualActivityAsync(
            int asdPersonId,
            PlanDayParameters parameters,
            DateTime currentDate,
            CancellationToken cancellationToken = default);

        // Kept for the legacy MVC controller. API completion uses the transactional methods above.
        void DonePoint(int taskId, int? pointId);

        ActualActivityVM CreateActualActivity(int asdPersonId, int? currentTaskId, PlanDayParameters parameters, DateTime currentDate);
    }

    public sealed class ActivityCompletionResult
    {
        public bool Found { get; init; }

        public int TaskId { get; init; }

        public int? PointId { get; init; }

        public bool AlreadyCompleted { get; init; }

        public bool TaskCompleted { get; init; }

        public int? CurrentTaskId { get; init; }
    }
}
