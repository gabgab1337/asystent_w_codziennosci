using AssistantLogic.ViewModel;

namespace Assistant.Api.Contracts
{
    public class PlanTaskDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public RealizationState State { get; set; }

        /// <summary>Wall clock "HH:mm" as computed by the plan services.</summary>
        public string? TimeBegin { get; set; }

        public string? TimeEnd { get; set; }

        public int DurationMinutes { get; set; }

        public bool AnchorBegin { get; set; }

        public bool AnchorEnd { get; set; }

        /// <summary>Polish text about being ahead of or behind the planned duration.</summary>
        public string? Message { get; set; }

        public bool MessagePositive { get; set; }

        public List<PlanPointDto> Points { get; set; } = new List<PlanPointDto>();

        public static PlanTaskDto From(PlanDayTaskVM task)
        {
            return new PlanTaskDto
            {
                Id = task.Id,
                Name = task.Name,
                Description = PlanText.ToPlainText(task.Description),
                State = task.State,
                TimeBegin = task.TimeBegin,
                TimeEnd = task.TimeEnd,
                DurationMinutes = task.Time,
                AnchorBegin = task.AnchorBegin,
                AnchorEnd = task.AnchorEnd,
                Message = string.IsNullOrWhiteSpace(task.Message) ? null : task.Message.Trim(),
                MessagePositive = task.MessagePositiv,
                Points = (task.Points ?? new List<PlanDayPointVM>())
                    .Select(PlanPointDto.From)
                    .ToList()
            };
        }
    }
}
