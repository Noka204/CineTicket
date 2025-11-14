using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CineTicket.Utils
{
    public static class PromoTextUtil
    {
        private static readonly Regex _az09 = new Regex("[^A-Z0-9]", RegexOptions.Compiled);

        public static string NormalizeCode(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // bỏ dấu tiếng Việt
            string s = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in s)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            s = sb.ToString().Normalize(NormalizationForm.FormC);

            // upper + lọc ký tự
            s = s.ToUpperInvariant();
            s = _az09.Replace(s, "");
            return s;
        }
    }
}
