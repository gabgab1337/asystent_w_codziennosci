using AssistantDatabase.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using AssistantLogic.Triggers.TriggersTime;
using AssistantLogic.Triggers.Weather;
using AssistantLogic.Model;
using AssistantLogic.Validators;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AssistantLogic.ViewModel
{
    public class TriggerAddEditVM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool EditValidationMode { get; set; } = false;

        [Required(ErrorMessage = "Pole \"Nazwa wyzwalacza\" jest wymagane")]
        [StringLength(100)] 
        public string Name { get; set; }

        [EnumIsRequired("Typ wyzwalacza", TriggerType.None)]
        public TriggerType Type { get; set; }

        [NotMapped]
        public List<SelectListItem> Types { get; set; }

        [EnumIsRequiredDependOn("Podtyp wyzwalacza czasu", TriggerTimeType.None, "Type", TriggerType.Time)]
        public TriggerTimeType TimeType { get; set; }

        [ValidateNever]
        public List<SelectListItem> TimeTypes { get; set; }

        //[EnumIsRequiredDependOn("Podtyp wyzwalacza pogody", WeatherType.None, "Type", TriggerType.Weather)]
        public WeatherType WeatherType { get; set; }

        [ValidateNever]
        public List<SelectListItem> WeatherTypes { get; set; }

        
        /// time
        
        [DataType(DataType.Date)]
        [FieldIsRequiredDependOn("Data", "TimeType", TriggerTimeType.Data)]
        public DateTime? SelectedDate { get; set; }

        [DataType(DataType.Date)]
        [FieldIsRequiredDependOn("Data początkowa", "TimeType", TriggerTimeType.DateRange)]
        public DateTime? BeginDate { get; set; }

        [DataType(DataType.Date)]
        [FieldIsRequiredDependOn("Data końcowa", "TimeType", TriggerTimeType.DateRange)]
        [FielsIdIsBiggestOrSameDependOn("Data końcowa", "BeginDate", "Data początkowa", "TimeType", TriggerTimeType.DateRange)]
        public DateTime? EndDate { get; set; }
        
        [EnumIsRequiredDependOn("Dzień tygodnia", TriggerWeekDayType.None, "TimeType", TriggerTimeType.WeekDay)]
        public TriggerWeekDayType? WeekDayType { get; set; }

        [ValidateNever]
        public List<SelectListItem> WeekDayTypes { get; set; }

        [DataType(DataType.Date)]
        [FieldIsRequiredDependOn("Data początkowa", "TimeType", TriggerTimeType.DateInterval)]
        public DateTime? SelectedDateStartInterval { get; set; }

        [EnumIsRequiredDependOn("Odstęp czasowy", TriggerDateIntervalType.None, "TimeType", TriggerTimeType.DateInterval)]
        public TriggerDateIntervalType? DateIntervalType { get; set; }

        [ValidateNever]
        public List<SelectListItem> DateIntervalTypes { get; set; }


        
        

        //Weather

        [FieldIsRequiredDependOn("Temperatura: od", "Type", TriggerType.Weather)]
        public int? MinTemperature { get; set; }
        
        [FieldIsRequiredDependOn("Temperatura: do", "Type", TriggerType.Weather)]
        [FielsIdIsBiggestOrSameDependOn("Temperatura do", "MinTemperature", "Temperatura od", "Type", TriggerType.Weather)]
        public int? MaxTemperature { get; set; }
        public RainLevel? MinRainFall { get; set; }

        [FielsIdIsBiggestOrSameDependOn("Opady deszczu do", "MinRainFall", "Opady deszczu od", "WeatherType", WeatherType.Rain)]
        public RainLevel? MaxRainFall { get; set; }
        public SnowLevel? MinSnowFall { get; set; }

        [FielsIdIsBiggestOrSameDependOn("Opady śniegu do", "MinSnowFall", "Opady śniegu od", "WeatherType", WeatherType.Snow)]
        public SnowLevel? MaxSnowFall { get; set; }

        public ScaleWind? MinWindPower { get; set; }

        
        [FielsIdIsBiggestOrSameDependOn("Siła wiatru do", "MinWindPower", "Siła wiatru od", "Type", TriggerType.Weather)]
        public ScaleWind? MaxWindPower { get; set; }

        [ValidateNever]
        public List<SelectListItem> RainLevels { get; set; }
        [ValidateNever]
        public List<SelectListItem> SnowLevels { get; set; }
        [ValidateNever]
        public List<SelectListItem> WindLevels { get; set; }

        






    }
}
