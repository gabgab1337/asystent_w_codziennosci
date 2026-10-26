using Assistant.Api.Contracts;
using AssistantDatabase.Model;
using AssistantLogic.Model;
using AssistantLogic.Triggers;
using AssistantLogic.Triggers.Time;
using AssistantLogic.Triggers.TriggersTime;
using AssistantLogic.Triggers.Weather;
using AssistantLogic.ViewModel;

namespace Assistant.Api.Mapping
{
    public static class TriggerMapper
    {
        public static TriggerDto ToDto(TriggerDM entity)
        {
            ITrigger trigger = TriggerCreator.CreateTrigger(entity)
                ?? throw new NotSupportedException($"Trigger type \"{entity.Type}\" is not supported.");
            TriggerAddEditVM model = trigger.VM;

            return entity.Type switch
            {
                TriggerType.Time => new TimeTriggerDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Time = ToTimeDto(model)
                },
                TriggerType.Weather => new WeatherTriggerDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Weather = ToWeatherDto(model, (WeatherType)entity.SubType)
                },
                _ => throw new NotSupportedException($"Trigger type \"{entity.Type}\" is not supported.")
            };
        }

        public static TriggerDM ToEntity(CreateTriggerRequest request)
        {
            TriggerAddEditVM model = request switch
            {
                CreateTimeTriggerRequest time => ToViewModel(time.Name, time.Time),
                CreateWeatherTriggerRequest weather => ToViewModel(weather.Name, weather.Weather),
                _ => throw new NotSupportedException("Trigger kind is not supported.")
            };

            return TriggerCreator.ConvertToDM(model)
                ?? throw new NotSupportedException("Trigger kind is not supported.");
        }

        public static TriggerDM ToEntity(UpdateTriggerRequest request, int id)
        {
            TriggerAddEditVM model = request switch
            {
                UpdateTimeTriggerRequest time => ToViewModel(time.Name, time.Time),
                UpdateWeatherTriggerRequest weather => ToViewModel(weather.Name, weather.Weather),
                _ => throw new NotSupportedException("Trigger kind is not supported.")
            };
            model.Id = id;

            return TriggerCreator.ConvertToDM(model)
                ?? throw new NotSupportedException("Trigger kind is not supported.");
        }

        private static TriggerAddEditVM ToViewModel(string name, TimeTriggerDataRequest data)
        {
            TriggerAddEditVM model = new()
            {
                Name = name,
                Type = TriggerType.Time,
                TimeType = data.Subtype switch
                {
                    TimeTriggerSubtype.Date => TriggerTimeType.Data,
                    TimeTriggerSubtype.DateRange => TriggerTimeType.DateRange,
                    TimeTriggerSubtype.WeekDay => TriggerTimeType.WeekDay,
                    TimeTriggerSubtype.DateInterval => TriggerTimeType.DateInterval,
                    TimeTriggerSubtype.WorkingDay => TriggerTimeType.WorkingDay,
                    TimeTriggerSubtype.Weekend => TriggerTimeType.Weekend,
                    _ => throw new NotSupportedException($"Time trigger subtype \"{data.Subtype}\" is not supported.")
                },
                SelectedDate = ToDateTime(data.Date),
                BeginDate = ToDateTime(data.StartDate),
                EndDate = ToDateTime(data.EndDate),
                SelectedDateStartInterval = ToDateTime(data.StartDate),
                WeekDayType = data.WeekDay == null
                    ? null
                    : (TriggerWeekDayType)(int)data.WeekDay.Value,
                DateIntervalType = data.Interval == null
                    ? null
                    : (TriggerDateIntervalType)(int)data.Interval.Value
            };

            return model;
        }

        private static TriggerAddEditVM ToViewModel(string name, WeatherTriggerDataRequest data)
        {
            return new TriggerAddEditVM
            {
                Name = name,
                Type = TriggerType.Weather,
                WeatherType = data.Subtype switch
                {
                    WeatherTriggerSubtype.Default => WeatherType.Default,
                    WeatherTriggerSubtype.Rain => WeatherType.Rain,
                    WeatherTriggerSubtype.Snow => WeatherType.Snow,
                    _ => throw new NotSupportedException(
                        $"Weather trigger subtype \"{data.Subtype}\" is not supported.")
                },
                MinTemperature = data.MinTemperature,
                MaxTemperature = data.MaxTemperature,
                MinRainFall = data.MinRain,
                MaxRainFall = data.MaxRain,
                MinSnowFall = data.MinSnow,
                MaxSnowFall = data.MaxSnow,
                MinWindPower = data.MinWind,
                MaxWindPower = data.MaxWind
            };
        }

        private static TimeTriggerDataDto ToTimeDto(TriggerAddEditVM model)
        {
            return new TimeTriggerDataDto
            {
                Subtype = model.TimeType switch
                {
                    TriggerTimeType.Data => TimeTriggerSubtype.Date,
                    TriggerTimeType.DateRange => TimeTriggerSubtype.DateRange,
                    TriggerTimeType.WeekDay => TimeTriggerSubtype.WeekDay,
                    TriggerTimeType.DateInterval => TimeTriggerSubtype.DateInterval,
                    TriggerTimeType.WorkingDay => TimeTriggerSubtype.WorkingDay,
                    TriggerTimeType.Weekend => TimeTriggerSubtype.Weekend,
                    _ => throw new NotSupportedException(
                        $"Time trigger subtype \"{model.TimeType}\" is not supported.")
                },
                Date = ToDateOnly(model.SelectedDate),
                StartDate = model.TimeType == TriggerTimeType.DateRange
                    ? ToDateOnly(model.BeginDate)
                    : ToDateOnly(model.SelectedDateStartInterval),
                EndDate = ToDateOnly(model.EndDate),
                WeekDay = model.WeekDayType == null
                    ? null
                    : (TriggerWeekDay)(int)model.WeekDayType.Value,
                Interval = model.DateIntervalType == null
                    ? null
                    : (TriggerDateInterval)(int)model.DateIntervalType.Value
            };
        }

        private static WeatherTriggerDataDto ToWeatherDto(TriggerAddEditVM model, WeatherType weatherType)
        {
            return new WeatherTriggerDataDto
            {
                Subtype = weatherType switch
                {
                    WeatherType.Default => WeatherTriggerSubtype.Default,
                    WeatherType.Rain => WeatherTriggerSubtype.Rain,
                    WeatherType.Snow => WeatherTriggerSubtype.Snow,
                    _ => throw new NotSupportedException(
                        $"Weather trigger subtype \"{weatherType}\" is not supported.")
                },
                MinTemperature = model.MinTemperature
                    ?? throw InvalidLegacyData("minimum temperature"),
                MaxTemperature = model.MaxTemperature
                    ?? throw InvalidLegacyData("maximum temperature"),
                MinRain = model.MinRainFall,
                MaxRain = model.MaxRainFall,
                MinSnow = model.MinSnowFall,
                MaxSnow = model.MaxSnowFall,
                MinWind = model.MinWindPower
                    ?? throw InvalidLegacyData("minimum wind"),
                MaxWind = model.MaxWindPower
                    ?? throw InvalidLegacyData("maximum wind")
            };
        }

        private static DateTime? ToDateTime(DateOnly? date)
        {
            return date?.ToDateTime(TimeOnly.MinValue);
        }

        private static DateOnly? ToDateOnly(DateTime? date)
        {
            return date == null ? null : DateOnly.FromDateTime(date.Value);
        }

        private static InvalidOperationException InvalidLegacyData(string field)
        {
            return new InvalidOperationException($"Legacy trigger data is missing {field}.");
        }
    }
}
