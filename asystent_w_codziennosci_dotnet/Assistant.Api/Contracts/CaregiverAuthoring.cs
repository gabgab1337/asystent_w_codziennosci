using System.ComponentModel.DataAnnotations;

namespace Assistant.Api.Contracts
{
    public sealed class TaskDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int DurationMinutes { get; set; }

        public bool AnchorBegin { get; set; }

        public string? TimeBegin { get; set; }

        public bool AnchorEnd { get; set; }

        public string? TimeEnd { get; set; }

        public int Number { get; set; }
    }

    public sealed class TaskDetailsDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int DurationMinutes { get; set; }

        public bool AnchorBegin { get; set; }

        public string? TimeBegin { get; set; }

        public bool AnchorEnd { get; set; }

        public string? TimeEnd { get; set; }

        public int Number { get; set; }

        public List<PointDto> Points { get; set; } = new();

        public List<AttachedTriggerDto> Triggers { get; set; } = new();
    }

    public sealed class CreateTaskRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int DurationMinutes { get; set; }

        [RegularExpression(
            @"^(?:[01]\d|2[0-3]):[0-5]\d$",
            ErrorMessage = "Czas musi mieć format HH:mm.")]
        public string? TimeBegin { get; set; }

        [RegularExpression(
            @"^(?:[01]\d|2[0-3]):[0-5]\d$",
            ErrorMessage = "Czas musi mieć format HH:mm.")]
        public string? TimeEnd { get; set; }
    }

    public sealed class UpdateTaskRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int DurationMinutes { get; set; }

        [RegularExpression(
            @"^(?:[01]\d|2[0-3]):[0-5]\d$",
            ErrorMessage = "Czas musi mieć format HH:mm.")]
        public string? TimeBegin { get; set; }

        [RegularExpression(
            @"^(?:[01]\d|2[0-3]):[0-5]\d$",
            ErrorMessage = "Czas musi mieć format HH:mm.")]
        public string? TimeEnd { get; set; }
    }

    public sealed class PointDto
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Number { get; set; }
    }

    public sealed class PointDetailsDto
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Number { get; set; }

        public List<AttachedTriggerDto> Triggers { get; set; } = new();
    }

    public sealed class CreatePointRequest
    {
        [Required]
        [StringLength(300)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }
    }

    public sealed class UpdatePointRequest
    {
        [Required]
        [StringLength(300)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }
    }

    public sealed class AttachedTriggerDto
    {
        public TriggerDto Trigger { get; set; } = null!;

        public bool Negation { get; set; }
    }

    public sealed class AttachTriggerRequest
    {
        [Range(1, int.MaxValue)]
        public int TriggerId { get; set; }

        public bool Negation { get; set; }
    }

    public sealed class MoveTaskRequest
    {
        [Required]
        [EnumDataType(typeof(MoveDirection))]
        public MoveDirection? Direction { get; set; }

        [EnumDataType(typeof(TriggerWeekDay))]
        public TriggerWeekDay? WeekDay { get; set; }
    }

    public sealed class MovePointRequest
    {
        [Required]
        [EnumDataType(typeof(MoveDirection))]
        public MoveDirection? Direction { get; set; }
    }

    public enum MoveDirection
    {
        Up = 1,
        Down
    }
}
