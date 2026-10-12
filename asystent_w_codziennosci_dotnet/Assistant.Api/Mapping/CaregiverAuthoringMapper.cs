using Assistant.Api.Contracts;
using Assistant.Api.Storage;
using AssistantDatabase.Model;
using AssistantLogic.ViewModel;

namespace Assistant.Api.Mapping
{
    public static class CaregiverAuthoringMapper
    {
        public static TaskDto ToTaskDto(TaskDM task)
        {
            return new TaskDto
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                DurationMinutes = task.Time,
                AnchorBegin = task.AnchorBegin,
                TimeBegin = FormatTime(task.TimeBegin),
                AnchorEnd = task.AnchorEnd,
                TimeEnd = FormatTime(task.TimeEnd),
                Number = task.Number
            };
        }

        public static TaskDetailsDto ToTaskDetailsDto(
            TaskDM task,
            IEnumerable<TaskPointDM> points,
            IEnumerable<TriggerDM> triggers)
        {
            return new TaskDetailsDto
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                DurationMinutes = task.Time,
                AnchorBegin = task.AnchorBegin,
                TimeBegin = FormatTime(task.TimeBegin),
                AnchorEnd = task.AnchorEnd,
                TimeEnd = FormatTime(task.TimeEnd),
                Number = task.Number,
                Points = points.Select(ToPointDto).ToList(),
                Triggers = triggers.Select(ToAttachedTriggerDto).ToList()
            };
        }

        public static PointDto ToPointDto(TaskPointDM point)
        {
            return new PointDto
            {
                Id = point.Id,
                TaskId = point.TaskId,
                Name = point.Name,
                Description = point.Description,
                Number = point.Number,
                PhotoUrl = PhotoPaths.ToDownloadPath(point.PhotoUrl)
            };
        }

        public static PointDetailsDto ToPointDetailsDto(
            TaskPointDM point,
            IEnumerable<TriggerDM> triggers)
        {
            return new PointDetailsDto
            {
                Id = point.Id,
                TaskId = point.TaskId,
                Name = point.Name,
                Description = point.Description,
                Number = point.Number,
                PhotoUrl = PhotoPaths.ToDownloadPath(point.PhotoUrl),
                Triggers = triggers.Select(ToAttachedTriggerDto).ToList()
            };
        }

        public static TaskAddEditVM ToViewModel(CreateTaskRequest request)
        {
            return ToTaskViewModel(
                id: 0,
                request.Name,
                request.Description,
                request.DurationMinutes,
                request.TimeBegin,
                request.TimeEnd);
        }

        public static TaskAddEditVM ToViewModel(int id, UpdateTaskRequest request)
        {
            return ToTaskViewModel(
                id,
                request.Name,
                request.Description,
                request.DurationMinutes,
                request.TimeBegin,
                request.TimeEnd);
        }

        public static PointAddEditVM ToViewModel(int taskId, CreatePointRequest request)
        {
            return new PointAddEditVM
            {
                TaskId = taskId,
                Name = request.Name,
                Description = request.Description
            };
        }

        public static PointAddEditVM ToViewModel(TaskPointDM existing, UpdatePointRequest request)
        {
            return new PointAddEditVM
            {
                Id = existing.Id,
                TaskId = existing.TaskId,
                Number = existing.Number,
                Name = request.Name,
                Description = request.Description,
                PhotoUrl = existing.PhotoUrl
            };
        }

        private static AttachedTriggerDto ToAttachedTriggerDto(TriggerDM trigger)
        {
            return new AttachedTriggerDto
            {
                Trigger = TriggerMapper.ToDto(trigger),
                Negation = trigger.Negation
            };
        }

        private static TaskAddEditVM ToTaskViewModel(
            int id,
            string name,
            string? description,
            int durationMinutes,
            string? timeBegin,
            string? timeEnd)
        {
            bool hasBeginAnchor = !string.IsNullOrWhiteSpace(timeBegin);
            bool hasEndAnchor = !string.IsNullOrWhiteSpace(timeEnd);

            return new TaskAddEditVM
            {
                Id = id,
                Name = name,
                Description = description,
                Hours = durationMinutes / 60,
                Minutes = durationMinutes % 60,
                AnchorBegin = hasBeginAnchor,
                TimeBegin = hasBeginAnchor ? timeBegin : null,
                AnchorEnd = hasEndAnchor,
                TimeEnd = hasEndAnchor ? timeEnd : null
            };
        }

        private static string? FormatTime(int? minutes)
        {
            return minutes == null
                ? null
                : $"{minutes.Value / 60:D2}:{minutes.Value % 60:D2}";
        }
    }
}
