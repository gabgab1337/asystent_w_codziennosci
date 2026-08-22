namespace AssistantLogic.Common
{
    public static class StringExtensions
    {
        public static string ToHTML(this string text)
        {
            while(text.Contains("\n"))
            {
                text = text.Replace("\n", "<br>");
            }
            
            return text;
        }

       
    }
}
