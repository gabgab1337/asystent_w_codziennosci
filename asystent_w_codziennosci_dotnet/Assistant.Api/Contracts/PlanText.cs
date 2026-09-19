namespace Assistant.Api.Contracts
{
    internal static class PlanText
    {
        /// <summary>
        /// The plan services run descriptions through <c>ToHTML()</c>, which only turns newlines
        /// into &lt;br&gt;. Clients render their own markup, so undo it and return plain text.
        /// </summary>
        public static string? ToPlainText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return text.Replace("<br>", "\n");
        }
    }
}
