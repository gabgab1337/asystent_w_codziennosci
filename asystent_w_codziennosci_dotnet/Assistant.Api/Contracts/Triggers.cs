using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AssistantLogic.Triggers.Weather;

namespace Assistant.Api.Contracts
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(TimeTriggerDto), "time")]
    [JsonDerivedType(typeof(WeatherTriggerDto), "weather")]
    public abstract class TriggerDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public sealed class TimeTriggerDto : TriggerDto
    {
        public TimeTriggerDataDto Time { get; set; } = new();
    }

    public sealed class WeatherTriggerDto : TriggerDto
    {
        public WeatherTriggerDataDto Weather { get; set; } = new();
    }

    public sealed class TimeTriggerDataDto
    {
        public TimeTriggerSubtype Subtype { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateOnly? Date { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateOnly? StartDate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateOnly? EndDate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TriggerWeekDay? WeekDay { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TriggerDateInterval? Interval { get; set; }
    }

    public sealed class WeatherTriggerDataDto
    {
        public WeatherTriggerSubtype Subtype { get; set; }

        public int MinTemperature { get; set; }

        public int MaxTemperature { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RainLevel? MinRain { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RainLevel? MaxRain { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SnowLevel? MinSnow { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SnowLevel? MaxSnow { get; set; }

        public ScaleWind MinWind { get; set; }

        public ScaleWind MaxWind { get; set; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(CreateTimeTriggerRequest), "time")]
    [JsonDerivedType(typeof(CreateWeatherTriggerRequest), "weather")]
    public abstract class CreateTriggerRequest
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
    }

    public sealed class CreateTimeTriggerRequest : CreateTriggerRequest
    {
        [Required]
        public TimeTriggerDataRequest Time { get; set; } = null!;
    }

    public sealed class CreateWeatherTriggerRequest : CreateTriggerRequest
    {
        [Required]
        public WeatherTriggerDataRequest Weather { get; set; } = null!;
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(UpdateTimeTriggerRequest), "time")]
    [JsonDerivedType(typeof(UpdateWeatherTriggerRequest), "weather")]
    public abstract class UpdateTriggerRequest
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
    }

    public sealed class UpdateTimeTriggerRequest : UpdateTriggerRequest
    {
        [Required]
        public TimeTriggerDataRequest Time { get; set; } = null!;
    }

    public sealed class UpdateWeatherTriggerRequest : UpdateTriggerRequest
    {
        [Required]
        public WeatherTriggerDataRequest Weather { get; set; } = null!;
    }

    public sealed class TimeTriggerDataRequest : IValidatableObject
    {
        public TimeTriggerSubtype Subtype { get; set; }

        public DateOnly? Date { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public TriggerWeekDay? WeekDay { get; set; }

        public TriggerDateInterval? Interval { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Enum.IsDefined(Subtype))
            {
                yield return new ValidationResult(
                    "Nieobsługiwany podtyp wyzwalacza czasu.",
                    new[] { nameof(Subtype) });
                yield break;
            }

            switch (Subtype)
            {
                case TimeTriggerSubtype.Date when Date == null:
                    yield return Required(nameof(Date));
                    break;
                case TimeTriggerSubtype.DateRange:
                    if (StartDate == null)
                    {
                        yield return Required(nameof(StartDate));
                    }

                    if (EndDate == null)
                    {
                        yield return Required(nameof(EndDate));
                    }
                    else if (StartDate != null && EndDate < StartDate)
                    {
                        yield return new ValidationResult(
                            "Data końcowa nie może być wcześniejsza niż data początkowa.",
                            new[] { nameof(EndDate) });
                    }
                    break;
                case TimeTriggerSubtype.WeekDay when WeekDay == null:
                    yield return Required(nameof(WeekDay));
                    break;
                case TimeTriggerSubtype.DateInterval:
                    if (StartDate == null)
                    {
                        yield return Required(nameof(StartDate));
                    }

                    if (Interval == null)
                    {
                        yield return Required(nameof(Interval));
                    }
                    break;
            }
        }

        private static ValidationResult Required(string memberName)
        {
            return new ValidationResult(
                $"Pole \"{memberName}\" jest wymagane dla wybranego podtypu.",
                new[] { memberName });
        }
    }

    public sealed class WeatherTriggerDataRequest : IValidatableObject
    {
        public WeatherTriggerSubtype Subtype { get; set; }

        public int? MinTemperature { get; set; }

        public int? MaxTemperature { get; set; }

        public RainLevel? MinRain { get; set; }

        public RainLevel? MaxRain { get; set; }

        public SnowLevel? MinSnow { get; set; }

        public SnowLevel? MaxSnow { get; set; }

        public ScaleWind? MinWind { get; set; }

        public ScaleWind? MaxWind { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Enum.IsDefined(Subtype))
            {
                yield return new ValidationResult(
                    "Nieobsługiwany podtyp wyzwalacza pogody.",
                    new[] { nameof(Subtype) });
                yield break;
            }

            if (MinTemperature == null)
            {
                yield return Required(nameof(MinTemperature));
            }

            if (MaxTemperature == null)
            {
                yield return Required(nameof(MaxTemperature));
            }
            else if (MinTemperature != null && MaxTemperature < MinTemperature)
            {
                yield return InvalidRange(nameof(MaxTemperature));
            }

            if (MinWind == null)
            {
                yield return Required(nameof(MinWind));
            }

            if (MaxWind == null)
            {
                yield return Required(nameof(MaxWind));
            }
            else if (MinWind != null && MaxWind < MinWind)
            {
                yield return InvalidRange(nameof(MaxWind));
            }

            if (Subtype == WeatherTriggerSubtype.Rain)
            {
                if (MinRain == null)
                {
                    yield return Required(nameof(MinRain));
                }

                if (MaxRain == null)
                {
                    yield return Required(nameof(MaxRain));
                }
                else if (MinRain != null && MaxRain < MinRain)
                {
                    yield return InvalidRange(nameof(MaxRain));
                }
            }

            if (Subtype == WeatherTriggerSubtype.Snow)
            {
                if (MinSnow == null)
                {
                    yield return Required(nameof(MinSnow));
                }

                if (MaxSnow == null)
                {
                    yield return Required(nameof(MaxSnow));
                }
                else if (MinSnow != null && MaxSnow < MinSnow)
                {
                    yield return InvalidRange(nameof(MaxSnow));
                }
            }
        }

        private static ValidationResult Required(string memberName)
        {
            return new ValidationResult(
                $"Pole \"{memberName}\" jest wymagane.",
                new[] { memberName });
        }

        private static ValidationResult InvalidRange(string memberName)
        {
            return new ValidationResult(
                "Maksymalna wartość nie może być mniejsza od minimalnej.",
                new[] { memberName });
        }
    }

    public enum TimeTriggerSubtype
    {
        Date = 1,
        DateRange,
        WeekDay,
        DateInterval,
        WorkingDay,
        Weekend
    }

    public enum TriggerWeekDay
    {
        Monday = 1,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }

    public enum TriggerDateInterval
    {
        EveryDay = 1,
        EveryTwoDays,
        EveryThreeDays,
        EveryTwoWeeks,
        EveryThreeWeeks,
        EveryMonth,
        EveryTwoMonths,
        EveryThreeMonths,
        EveryYear,
        Every2Years,
        EveryMaundyThursday,
        EveryGoodFriday,
        EveryHolySaturday,
        EveryEasterDay,
        EveryEasterMonday
    }

    public enum WeatherTriggerSubtype
    {
        Default = 1,
        Rain,
        Snow
    }
}
