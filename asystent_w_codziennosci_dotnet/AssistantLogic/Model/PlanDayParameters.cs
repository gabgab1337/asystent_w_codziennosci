namespace AssistantLogic.Model
{
    public class PlanDayParameters
    {
        private DateTime currentDate;
        private DateTime currentDateTime;

        public DateTime CurrentDate
        {
            get
            {
                return currentDate;
            }
            set
            {
                currentDate = new DateTime(value.Year, value.Month, value.Day);
            }
        }

        public DateTime CurrentDateTime
        {
            get
            {
                return currentDateTime;
            }
            set
            {
                currentDateTime = value;
                CurrentDate = value;
            }
        }


        
        public WeatherModel Weather { get; set; }
    }
}
