using AssistantDatabase.Model;
using AssistantLogic.Common;
using AssistantLogic.Model;
using AssistantLogic.Triggers.TriggersTime;
using AssistantLogic.ViewModel;
using System.Globalization;
using System.Runtime.CompilerServices;


namespace AssistantLogic.Triggers.Time
{
    //http://www.algorytm.org/przetwarzanie-dat/wyznaczanie-daty-wielkanocy-metoda-meeusa-jonesa-butchera.html
    public class TriggerTime : ITrigger
    {
        string name;

        TriggerTimeType timeType;
        DateTime? selectedDate;
        DateTime? beginDate;
        DateTime? endDate;
        DateTime? selectedDateStartInterval;
        TriggerWeekDayType? weekDay;
        TriggerDateIntervalType? dateInterval;
        string? data;
        bool negation;
        public TriggerTime(TriggerDM dm)
        {
            timeType = (TriggerTimeType)dm.SubType;
            name = dm.Name;
            data = dm.Data;
            negation = dm.Negation;
            switch (timeType)
            {
                case TriggerTimeType.Data:
                    selectedDate = ParseDateTime(dm.Data);
                    break;
                case TriggerTimeType.DateRange:
                    string[] dates = dm.Data.Split(";");
                    beginDate = ParseDateTime(dates[0]);
                    endDate = ParseDateTime(dates[1]);
                    break;
                case TriggerTimeType.WeekDay:
                    int data = int.Parse(dm.Data);
                    weekDay = (TriggerWeekDayType)data;
                    break;
                case TriggerTimeType.DateInterval:
                    string[] datesInterval = dm.Data.Split(";");
                    selectedDateStartInterval = ParseDateTime(datesInterval[0]);
                    int typeInterval = int.Parse(datesInterval[1]);
                    dateInterval = (TriggerDateIntervalType)typeInterval;
                    break;
                case TriggerTimeType.WorkingDay:
                    break;
                case TriggerTimeType.Weekend:
                    break;
            }
        }

        // public static TriggerDM ConvertToVM(TriggerAddEditVM vm)
        //{

        //}

        public static TriggerDM ConvertToDM(TriggerAddEditVM vm)
        {
            TriggerDM dm = new TriggerDM();
            dm.Id = vm.Id;
            dm.Name = vm.Name;
            dm.Type = TriggerType.Time;
            dm.SubType = (int)vm.TimeType;

            switch (vm.TimeType)
            {
                case TriggerTimeType.Data:
                    dm.Data = DateTimeToString(vm.SelectedDate);
                    break;
                case TriggerTimeType.DateRange:
                    dm.Data = DateTimeToString(vm.BeginDate) + ";" + DateTimeToString(vm.EndDate);
                    break;
                case TriggerTimeType.WeekDay:
                    dm.Data = ((int)(vm.WeekDayType.Value)).ToString();
                    break;
                case TriggerTimeType.DateInterval:
                    dm.Data = DateTimeToString(vm.SelectedDateStartInterval) + ";" + ((int)vm.DateIntervalType.Value).ToString();
                    break;
                case TriggerTimeType.WorkingDay:
                    dm.Data = null;
                    break;
                case TriggerTimeType.Weekend:
                    dm.Data = null;
                    break;
            }


            return dm;
        }

        public TriggerAddEditVM VM
        {
            get
            {
                TriggerAddEditVM vm = new TriggerAddEditVM();
                vm.Name = name;
                vm.Type = TriggerType.Time;
                vm.TimeType = timeType;

                switch (timeType)
                {
                    case TriggerTimeType.Data:
                        vm.SelectedDate = selectedDate;
                        break;
                    case TriggerTimeType.DateRange:
                        vm.BeginDate = beginDate;
                        vm.EndDate = endDate;
                        break;
                    case TriggerTimeType.WeekDay:
                        vm.WeekDayType = weekDay;
                        break;
                    case TriggerTimeType.DateInterval:
                        vm.SelectedDateStartInterval = selectedDateStartInterval;
                        vm.DateIntervalType = dateInterval;
                        break;
                    case TriggerTimeType.WorkingDay:
                        break;
                    case TriggerTimeType.Weekend:
                        break;
                }
                return vm;
            }
        }


        public string Description
        {
            get
            {
                string stringNegation = "";
                if (negation)
                {
                    stringNegation = "NIE";
                }


                switch (timeType)
                {
                    case TriggerTimeType.Data:
                        return $"{stringNegation} Uruchamia się dnia {DateTimeToShortDateString(selectedDate)}";
                    case TriggerTimeType.DateRange:
                        return $"{stringNegation} Uruchamia się w przedziale dat od dnia {DateTimeToShortDateString(beginDate)} do dnia {DateTimeToShortDateString(endDate)} włącznie";
                    case TriggerTimeType.WeekDay:
                        return $"{stringNegation} Uruchamia się jeżeli dzień tygodnia to {weekDay.Value.GetEnumDescription()}";
                    case TriggerTimeType.DateInterval:
                        return $"{stringNegation} Uruchamia się od {DateTimeToShortDateString(selectedDateStartInterval)} i występuje {dateInterval.Value.GetEnumDescription()} ";
                    case TriggerTimeType.WorkingDay:
                        return $"{stringNegation} Uruchamia się jeżeli jest to dzień tygodnia pracujący czyli od poniedziałku do piątku";
                    case TriggerTimeType.Weekend:
                        return $"{stringNegation} Uruchamia się jeżeli jest weekend czyli sobota lub niedziela";
                }
                return "brak opisu";
            }
        }

        public string ShortDescription
        {
            get
            {
                string stringNegation = "";
                if(negation)
                {
                    stringNegation = "NIE";
                }
                switch (timeType)
                {
                    case TriggerTimeType.Data:
                        return $"[{stringNegation} {DateTimeToShortDateString(selectedDate)}]";
                    case TriggerTimeType.DateRange:
                        return $"[{stringNegation} {DateTimeToShortDateString(beginDate)}, {DateTimeToShortDateString(endDate)}]";
                    case TriggerTimeType.WeekDay:
                        return $"[{stringNegation} {weekDay.Value.GetEnumDescription()}]";
                    case TriggerTimeType.DateInterval:
                        return $"[{stringNegation} {dateInterval.Value.GetEnumDescription()} od {DateTimeToShortDateString(selectedDateStartInterval)}]";
                    case TriggerTimeType.WorkingDay:
                        return $"{stringNegation} [poniedziałek, piątek]";
                    case TriggerTimeType.Weekend:
                        return $"{stringNegation} [sobota,niedziela]";
                }
                return "brak opisu";
            }
        }
        public string SubTypeName
        {
            get
            {
                return timeType.GetEnumDescription();
            }
        }

        public bool IsRun(PlanDayParameters parameters)
        {
            DateTime currentDate = parameters.CurrentDate;
            WeatherModel weather = parameters.Weather;
            // DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            switch (timeType)
            {
                case TriggerTimeType.Data:
                    return selectedDate == currentDate;
                case TriggerTimeType.DateRange:
                    return beginDate <= currentDate && currentDate <= endDate;
                case TriggerTimeType.WeekDay:

                    return (currentDate.DayOfWeek.ToString() == weekDay.ToString());
                /*
                return ((today.DayOfWeek == DayOfWeek.Monday && weekDay == TriggerWeekDayType.Monday)
                    || ((today.DayOfWeek == DayOfWeek.Tuesday && weekDay == TriggerWeekDayType.Tuesday)
                    || ((today.DayOfWeek == DayOfWeek.Wednesday && weekDay == TriggerWeekDayType.Wednesday)
                    || ((today.DayOfWeek == DayOfWeek.Thursday && weekDay == TriggerWeekDayType.Thursday)
                    || ((today.DayOfWeek == DayOfWeek.Friday && weekDay == TriggerWeekDayType.Friday)
                    || ((today.DayOfWeek == DayOfWeek.Saturday && weekDay == TriggerWeekDayType.Saturday)
                    || ((today.DayOfWeek == DayOfWeek.Saturday && weekDay == TriggerWeekDayType.Saturday)
                    */
                case TriggerTimeType.DateInterval:
                    return IsInDateInterval(selectedDateStartInterval.Value.Date, dateInterval.Value, currentDate);
                case TriggerTimeType.WorkingDay:
                    return (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday);
                case TriggerTimeType.Weekend:
                    return currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday;
                default:
                    return false;
            }
        }

        public bool Negation
        {
            get
            {
                return negation;
            }
        }

        public bool IsWorkinForDayWeek(TriggerWeekDayType weekDay)
        {
            switch(timeType)
            {
                case TriggerTimeType.Data:
                    return ConvertToTriggerWeekDay(selectedDate)== weekDay;
                case TriggerTimeType.DateRange:
                    TriggerWeekDayType beginWeekDay = ConvertToTriggerWeekDay(beginDate);
                    TriggerWeekDayType endWeekDay = ConvertToTriggerWeekDay(endDate);

                    for(int i=0; i<7; i++)
                    {
                        TriggerWeekDayType testWeekDay = AddDaysToTriggerWeekDayType(beginWeekDay, i);
                        if(testWeekDay == weekDay)
                        {
                            return true;
                        }
                        if(testWeekDay == endWeekDay)
                        {
                            break;
                        }
                    }
                    return false;

                case TriggerTimeType.WeekDay:
                    return this.weekDay == weekDay;
                case TriggerTimeType.DateInterval:
                    IsWorkinForDayWeekInDateInterval(weekDay, selectedDateStartInterval.Value, dateInterval.Value);
                    break;
                case TriggerTimeType.WorkingDay:
                    return weekDay == TriggerWeekDayType.Monday
                        || weekDay == TriggerWeekDayType.Tuesday
                        || weekDay == TriggerWeekDayType.Wednesday
                        || weekDay == TriggerWeekDayType.Thursday
                        || weekDay == TriggerWeekDayType.Friday;
                case TriggerTimeType.Weekend:
                    return weekDay == TriggerWeekDayType.Saturday ||
                        weekDay == TriggerWeekDayType.Sunday;
            }
            return false;
        }

        private bool IsWorkinForDayWeekInDateInterval(TriggerWeekDayType weekDay, DateTime date, TriggerDateIntervalType interval)
        {
            switch (interval)
            {
                case TriggerDateIntervalType.EveryDay:
                case TriggerDateIntervalType.EveryTwoDays:
                case TriggerDateIntervalType.EveryThreeDays:
                    return true;
                case TriggerDateIntervalType.EveryTwoWeeks:
                case TriggerDateIntervalType.EveryThreeWeeks:
                    return ConvertToTriggerWeekDay(date) == weekDay;
                case TriggerDateIntervalType.EveryMonth:
                case TriggerDateIntervalType.EveryTwoMonths:
                case TriggerDateIntervalType.EveryThreeMonths:
                case TriggerDateIntervalType.EveryYear:
                case TriggerDateIntervalType.Every2Years:
                    return true;
                case TriggerDateIntervalType.EveryMaundyThursday:
                    return weekDay == TriggerWeekDayType.Thursday;
                case TriggerDateIntervalType.EveryGoodFriday:
                    return weekDay == TriggerWeekDayType.Friday;
                case TriggerDateIntervalType.EveryHolySaturday:
                    return weekDay == TriggerWeekDayType.Saturday;
                case TriggerDateIntervalType.EveryEasterDay:
                    return weekDay == TriggerWeekDayType.Sunday;
                case TriggerDateIntervalType.EveryEasterMonday:
                    return weekDay == TriggerWeekDayType.Monday;
            }
             return false;
        }

        private TriggerWeekDayType AddDaysToTriggerWeekDayType(TriggerWeekDayType dateType, int days)
        {
            const int daysInWeek = 7;
            days = days % daysInWeek;

            int value = (int)dateType;
            int valueStartFor0 = value - 1;
            valueStartFor0 = (valueStartFor0 + days+ daysInWeek) % daysInWeek;
            value = valueStartFor0 + 1;
            return (TriggerWeekDayType)value;
        }
        private TriggerWeekDayType ConvertToTriggerWeekDay(DateTime? date)
        {
            if ((date==null))
            {
                return TriggerWeekDayType.None;
            }
            else
            {
                const int sunday = 7;
                int dayOfWeekValue = (int)date.Value.DayOfWeek;
                if (dayOfWeekValue == 0)
                {
                    dayOfWeekValue = sunday;
                }
                return (TriggerWeekDayType)dayOfWeekValue;
            }
            
        }

        private bool IsInDateInterval(DateTime date, TriggerDateIntervalType interval, DateTime currentDate)
        {
            while(date < currentDate)
            {
                switch(interval)
                {
                    case TriggerDateIntervalType.EveryDay:
                        return true;
                    case TriggerDateIntervalType.EveryTwoDays:
                        date = date.AddDays(2);
                        break;
                    case TriggerDateIntervalType.EveryThreeDays:
                        date = date.AddDays(3);
                        break;
                    case TriggerDateIntervalType.EveryTwoWeeks:
                        date = date.AddDays(14);
                        break;
                    case TriggerDateIntervalType.EveryThreeWeeks:
                        date = date.AddDays(21);
                        break;
                    case TriggerDateIntervalType.EveryMonth:
                        date = date.AddMonths(1);
                        break;
                    case TriggerDateIntervalType.EveryTwoMonths:
                        date = date.AddMonths(2);
                        break;
                    case TriggerDateIntervalType.EveryThreeMonths:
                        date = date.AddMonths(3);
                        break;
                    case TriggerDateIntervalType.EveryYear:
                        date = date.AddYears(1);
                        break;
                    case TriggerDateIntervalType.Every2Years:
                        date = date.AddYears(2);
                        break;
                    case TriggerDateIntervalType.EveryMaundyThursday:
                        date = EasterDate(date.Year, -3);
                        break;
                    case TriggerDateIntervalType.EveryGoodFriday:
                        date = EasterDate(date.Year, -2);
                        break;
                    case TriggerDateIntervalType.EveryHolySaturday:
                        date = EasterDate(date.Year, -1);
                        break;
                    case TriggerDateIntervalType.EveryEasterDay:
                        date = EasterDate(date.Year, 0);
                        break;
                    case TriggerDateIntervalType.EveryEasterMonday:
                        date = EasterDate(date.Year, 1);
                        break;
                }
            }
            return date == currentDate;
        }

        private DateTime ParseDateTime(string dateTimeAsText)
        {
            return DateTime.ParseExact(
                dateTimeAsText.Trim(),
                "dd.MM.yyyy HH:mm:ss",
                CultureInfo.InvariantCulture);
        }
        private static string? DateTimeToString(DateTime? date)
        {
            if (date.HasValue)
            {
                return date.Value.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            return null;
        }

        private static string? DateTimeToShortDateString(DateTime? date)
        {
            if (date.HasValue)
            {
                return date.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            return null;
        }


        private DateTime EasterDate(int year, int space)
        {
            
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int month = (h + l - 7 * m + 114) / 31;
            int day = ((h + l - 7 * m + 114) % 31) + 1;
            DateTime easter = new DateTime(year, month, day);
            easter = easter.AddDays(space);
            return easter;
        }
    }
}
