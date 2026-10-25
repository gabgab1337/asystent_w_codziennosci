using System.Net;
using System.Text.RegularExpressions;

namespace Assistant.Api.Contracts
{
    internal static class PlanText
    {
        private static readonly Regex BreakTag = new(
            @"<br\s*/?>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ParagraphClose = new(
            @"</p\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ParagraphOpen = new(
            @"<p\b[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AnyTag = new(@"<[^>]+>", RegexOptions.Compiled);

        /// <summary>
        /// Plan generation runs descriptions through <c>ToHTML()</c>, and older rows may already
        /// contain tags such as &lt;br&gt; or &lt;p&gt;. Clients render plain text, so turn those
        /// breaks into newlines and drop every other tag. The database value is left unchanged.
        /// </summary>
        public static string? ToPlainText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string plain = WebUtility.HtmlDecode(text);
            plain = BreakTag.Replace(plain, "\n");
            plain = ParagraphClose.Replace(plain, "\n");
            plain = ParagraphOpen.Replace(plain, string.Empty);
            plain = AnyTag.Replace(plain, string.Empty);
            plain = plain.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
            plain = plain.Trim();

            return plain.Length == 0 ? null : plain;
        }
    }
}
