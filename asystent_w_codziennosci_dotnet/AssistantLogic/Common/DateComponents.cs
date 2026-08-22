namespace AssistantLogic.Common
{
    public static class DateComponents
    {
        private static readonly string[] NameOfWeekDay = { "niedziela", "poniedziałek", "wtorek", "środa", "czwartek", "piątek", "sobota" };
        private static readonly string[] NameOfMonth = {"stycznia", "lutego", "marca", "kwietnia", "maja", "czerwca",
        "lipca", "sierpnia", "września", "października", "listopada", "grudnia"};
        private static object SetNumber(int condition)
        {
            object item = condition < 10 ? item = "0" + condition : item = condition;
            return item;
        }
        public static string AppearDate(DateTime time)
        {
            return NameOfWeekDay[(int)time.DayOfWeek] + ", " + time.Day + " " + NameOfMonth[time.Month - 1] + " " +
            time.Year + " " + SetNumber(time.Hour) + ":" + SetNumber(time.Minute) + ":" + SetNumber(time.Second);
        }
    }
}
