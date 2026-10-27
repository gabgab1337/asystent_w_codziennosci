using System.ComponentModel.DataAnnotations;
using System.Globalization;

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

    public sealed class CreateTaskRequest : IValidatableObject
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Optional when both timeBegin and timeEnd are supplied; in that case the
        /// duration is calculated as timeEnd - timeBegin.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int? DurationMinutes { get; set; }

        [RegularExpression(
            @"^(?:[01]\d|2[0-3]):[0-5]\d$",
            ErrorMessage = "Czas musi mieć format HH:mm.")]
        public string? TimeBegin { get; set; }

        [RegularExpression(
            @"^(?:[01]\d|2[0-3]):[0-5]\d$",
            ErrorMessage = "Czas musi mieć format HH:mm.")]
        public string? TimeEnd { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return TaskDurationContract.Validate(DurationMinutes, TimeBegin, TimeEnd);
        }

        internal int ResolveDurationMinutes()
        {
            return TaskDurationContract.Resolve(DurationMinutes, TimeBegin, TimeEnd);
        }
    }

    public sealed class UpdateTaskRequest : IValidatableObject
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Optional when both timeBegin and timeEnd are supplied; in that case the
        /// duration is calculated as timeEnd - timeBegin.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int? DurationMinutes { get; set; }

        [RegularExpression(
            @"^(?:[01]\d|2[0-3]):[0-5]\d$",
            ErrorMessage = "Czas musi mieć format HH:mm.")]
        public string? TimeBegin { get; set; }

        [RegularExpression(
            @"^(?:[01]\d|2[0-3]):[0-5]\d$",
            ErrorMessage = "Czas musi mieć format HH:mm.")]
        public string? TimeEnd { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return TaskDurationContract.Validate(DurationMinutes, TimeBegin, TimeEnd);
        }

        internal int ResolveDurationMinutes()
        {
            return TaskDurationContract.Resolve(DurationMinutes, TimeBegin, TimeEnd);
        }
    }

    internal static class TaskDurationContract
    {
        private const string FORMAT = @"hh\:mm";

        public static IEnumerable<ValidationResult> Validate(
            int? durationMinutes,
            string? timeBegin,
            string? timeEnd)
        {
            bool isClockPairIncomplete = string.IsNullOrWhiteSpace(timeBegin)
                || string.IsNullOrWhiteSpace(timeEnd);
            if (!durationMinutes.HasValue && isClockPairIncomplete)
            {
                yield return new ValidationResult(
                    "durationMinutes jest wymagane, jeśli nie podano zarówno timeBegin, jak i timeEnd.",
                    new[] { nameof(CreateTaskRequest.DurationMinutes) });
            }

            if (TryParse(timeBegin, out TimeSpan begin)
                && TryParse(timeEnd, out TimeSpan end)
                && end <= begin)
            {
                yield return new ValidationResult(
                    "timeEnd musi być późniejsze niż timeBegin.",
                    new[]
                    {
                        nameof(CreateTaskRequest.DurationMinutes),
                        nameof(CreateTaskRequest.TimeBegin),
                        nameof(CreateTaskRequest.TimeEnd)
                    });
            }
        }

        public static int Resolve(
            int? durationMinutes,
            string? timeBegin,
            string? timeEnd)
        {
            if (durationMinutes.HasValue)
            {
                return durationMinutes.Value;
            }

            if (TryParse(timeBegin, out TimeSpan begin)
                && TryParse(timeEnd, out TimeSpan end)
                && end > begin)
            {
                return (int)(end - begin).TotalMinutes;
            }

            throw new InvalidOperationException(
                "A task duration must be supplied or calculable from timeBegin and timeEnd.");
        }

        private static bool TryParse(string? value, out TimeSpan result)
        {
            return TimeSpan.TryParseExact(
                value,
                FORMAT,
                CultureInfo.InvariantCulture,
                out result);
        }
    }

    public sealed class PointDto
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Number { get; set; }

        public string? PhotoUrl { get; set; }
    }

    public sealed class PointDetailsDto
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Number { get; set; }

        public string? PhotoUrl { get; set; }

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
