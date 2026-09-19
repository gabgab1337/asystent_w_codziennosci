using AssistantLogic.Model;
using AssistantLogic.ViewModel;

namespace Assistant.Api.Contracts
{
    public class PlanDayResponse
    {
        /// <summary>The ASD person this plan belongs to.</summary>
        public int AsdPersonId { get; set; }

        public string AsdPersonName { get; set; } = string.Empty;

        /// <summary>The moment the plan was computed for, i.e. the plan parameters.</summary>
        public DateTimeOffset At { get; set; }

        /// <summary>
        /// Server clock at response time, so a client can show "minutes left" without
        /// trusting the device clock.
        /// </summary>
        public DateTimeOffset ServerTime { get; set; }

        public WeatherDto? Weather { get; set; }

        public List<PlanTaskDto> Tasks { get; set; } = new List<PlanTaskDto>();

        public static PlanDayResponse From(
            int asdPersonId,
            string asdPersonName,
            PlanDayParameters parameters,
            List<PlanDayTaskVM>? tasks)
        {
            return new PlanDayResponse
            {
                AsdPersonId = asdPersonId,
                AsdPersonName = asdPersonName,
                At = DateTime.SpecifyKind(parameters.CurrentDateTime, DateTimeKind.Local),
                ServerTime = DateTimeOffset.Now,
                Weather = parameters.Weather == null ? null : WeatherDto.From(parameters.Weather),
                Tasks = (tasks ?? new List<PlanDayTaskVM>())
                    .Select(PlanTaskDto.From)
                    .ToList()
            };
        }
    }
}
