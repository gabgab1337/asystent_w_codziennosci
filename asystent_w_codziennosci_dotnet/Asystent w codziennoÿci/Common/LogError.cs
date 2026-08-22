namespace Asystent_w_codzienności.Common
{
    public class ErrorLogger
    {
        public static void LogError(string error)
        {
            StreamWriter sw = new StreamWriter("ErrorLog.txt", true);
            sw.WriteLine(DateTime.Now.ToString() + " Error: " + error);
            sw.Close();
        }
    }
}
