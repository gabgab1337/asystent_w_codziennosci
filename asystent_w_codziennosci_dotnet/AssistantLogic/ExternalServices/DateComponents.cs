namespace AssistantDatabase.Services
{
    public class DateComponents
    {
        public static readonly string[] NameOfWeekDay = { "niedziela", "poniedziałek", "wtorek", "środa", "czwartek", "piątek", "sobota" };
        public static readonly string[] NameOfMonth = {"stycznia", "lutego", "marca", "kwietnia", "maja", "czerwca",
        "lipca", "sierpnia", "września", "października", "listopada", "grudnia"};
        public object SetNumber(int condition)
        {
            object item = condition < 10 ? item = "0" + condition : item = condition;
            return item;
        }
        public string AppearDate(DateTime czas)
        {
            return NameOfWeekDay[(int)czas.DayOfWeek] + ", " + czas.Day + " " + NameOfMonth[czas.Month - 1] + " " +
            czas.Year + " " + SetNumber(czas.Hour) + ":" + SetNumber(czas.Minute) + ":" + SetNumber(czas.Second);
        }
    }
}
