using System.ComponentModel.DataAnnotations;
using Assistant.Api.Storage;
using AssistantLogic.IInternalServices;
using AssistantLogic.ViewModel;

namespace Assistant.Api.Contracts
{
    public enum CurrentActivityKind
    {
        Task,
        FreeTime
    }

    /// <summary>
    /// Header states. <see cref="NothingToDo"/> replaces the old misspelled <c>NothinkToDo</c>,
    /// and <see cref="Normal"/> replaces <c>NormalWork</c>.
    /// </summary>
    public enum CurrentActivityState
    {
        Normal,
        LowTime,
        PostTime,
        NothingToDo
    }

    public class CurrentActivityResponse
    {
        public CurrentActivityKind Kind { get; set; }

        /// <summary>
        /// Null when <see cref="Kind"/> is <see cref="CurrentActivityKind.FreeTime"/>.
        /// The legacy placeholder task with id -1 is not returned.
        /// </summary>
        public CurrentActivityTaskDto? Task { get; set; }

        public string? PreviousTaskName { get; set; }

        public string? NextTaskName { get; set; }

        /// <summary>Server clock, so a client can show minutes left without trusting the device.</summary>
        public DateTimeOffset ServerTime { get; set; }

        public static CurrentActivityResponse From(ActualActivityVM activity)
        {
            ActualTaskVM? task = activity.TaskVM;
            if (IsFreeTime(task))
            {
                return new CurrentActivityResponse
                {
                    Kind = CurrentActivityKind.FreeTime,
                    Task = null,
                    PreviousTaskName = Clean(activity.TaskNameBefore),
                    NextTaskName = Clean(activity.TaskNameAfter),
                    ServerTime = DateTimeOffset.Now
                };
            }

            return new CurrentActivityResponse
            {
                Kind = CurrentActivityKind.Task,
                Task = CurrentActivityTaskDto.From(task!),
                PreviousTaskName = Clean(activity.TaskNameBefore),
                NextTaskName = Clean(activity.TaskNameAfter),
                ServerTime = DateTimeOffset.Now
            };
        }

        internal static bool IsFreeTime(ActualTaskVM? task)
        {
            return task == null || task.Id <= 0;
        }

        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }

    public class CurrentActivityTaskDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Number { get; set; }

        public bool AnchorBegin { get; set; }

        public bool AnchorEnd { get; set; }

        /// <summary>Wall clock "HH:mm".</summary>
        public string TimeBegin { get; set; } = string.Empty;

        public string TimeEnd { get; set; } = string.Empty;

        /// <summary>Minutes from midnight, matching the value the summary uses.</summary>
        public int TimeBeginMinutes { get; set; }

        public int TimeEndMinutes { get; set; }

        /// <summary>
        /// The point to complete next. Null when the only step is the synthetic stand-in
        /// for a task that has no points of its own; complete the task instead of a point.
        /// </summary>
        public int? CurrentPointId { get; set; }

        public List<CurrentActivityPointDto> Points { get; set; } = new List<CurrentActivityPointDto>();

        public static CurrentActivityTaskDto From(ActualTaskVM task)
        {
            return new CurrentActivityTaskDto
            {
                Id = task.Id,
                Name = task.Name ?? string.Empty,
                Description = PlanText.ToPlainText(task.Description),
                Number = task.Number,
                AnchorBegin = task.AnchorBegin,
                AnchorEnd = task.AnchorEnd,
                TimeBegin = task.TimeBegin ?? string.Empty,
                TimeEnd = task.TimeEnd ?? string.Empty,
                TimeBeginMinutes = task.TimeBeginValue,
                TimeEndMinutes = task.TimeEndValue,
                CurrentPointId = task.CurrentPointId,
                Points = (task.ActualPoints ?? new List<ActualPointVM>())
                    .Select(CurrentActivityPointDto.From)
                    .ToList()
            };
        }
    }

    public class CurrentActivityPointDto
    {
        /// <summary>Null for a synthetic point, because it has no row in TaskPoints.</summary>
        public int? Id { get; set; }

        public bool IsSynthetic { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? PhotoUrl { get; set; }

        public bool Done { get; set; }

        public bool Active { get; set; }

        public static CurrentActivityPointDto From(ActualPointVM point)
        {
            return new CurrentActivityPointDto
            {
                Id = point.Id,
                IsSynthetic = point.Id == null,
                Name = point.Name ?? string.Empty,
                Description = PlanText.ToPlainText(point.Description),
                PhotoUrl = PhotoPaths.ToDownloadPath(point.PhotoUrl),
                Done = point.Done,
                Active = point.Active
            };
        }
    }

    public class CurrentActivitySummaryResponse
    {
        /// <summary>Same window the MVC header treated as "time is running out".</summary>
        private const int LowTimeWithinMinutes = 5;

        public string? Name { get; set; }

        public CurrentActivityState State { get; set; }

        /// <summary>
        /// Minutes until the task's end. Negative after that end (<see cref="CurrentActivityState.PostTime"/>).
        /// Null when there is nothing left to do.
        /// </summary>
        public int? RemainingMinutes { get; set; }

        public DateTimeOffset ServerTime { get; set; }

        public static CurrentActivitySummaryResponse From(ActualActivityVM activity, int nowMinutes)
        {
            if (CurrentActivityResponse.IsFreeTime(activity.TaskVM))
            {
                return new CurrentActivitySummaryResponse
                {
                    Name = null,
                    State = CurrentActivityState.NothingToDo,
                    RemainingMinutes = null,
                    ServerTime = DateTimeOffset.Now
                };
            }

            ActualTaskVM task = activity.TaskVM!;
            int remaining = task.TimeEndValue - nowMinutes;
            CurrentActivityState state = remaining < 0
                ? CurrentActivityState.PostTime
                : remaining < LowTimeWithinMinutes
                    ? CurrentActivityState.LowTime
                    : CurrentActivityState.Normal;

            return new CurrentActivitySummaryResponse
            {
                Name = PlanText.ToPlainText(task.Name),
                State = state,
                RemainingMinutes = remaining,
                ServerTime = DateTimeOffset.Now
            };
        }
    }

    public class CompletePointRequest
    {
        /// <summary>Explicit point to mark done. Missing, null, and non-positive ids are rejected.</summary>
        [Range(1, int.MaxValue)]
        public int PointId { get; set; }
    }

    public class ActivityCompletionResponse
    {
        public int TaskId { get; set; }

        public int? PointId { get; set; }

        /// <summary>True when this exact point or task had already been completed today.</summary>
        public bool AlreadyCompleted { get; set; }

        /// <summary>True when the task is complete after this operation.</summary>
        public bool TaskCompleted { get; set; }

        /// <summary>
        /// Persisted pointer after the atomic operation. Null means the next activity GET must
        /// select the next weather/trigger-eligible task, or report free time.
        /// </summary>
        public int? CurrentTaskId { get; set; }

        public static ActivityCompletionResponse From(ActivityCompletionResult result)
        {
            return new ActivityCompletionResponse
            {
                TaskId = result.TaskId,
                PointId = result.PointId,
                AlreadyCompleted = result.AlreadyCompleted,
                TaskCompleted = result.TaskCompleted,
                CurrentTaskId = result.CurrentTaskId
            };
        }
    }
}
