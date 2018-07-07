using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ZumtenSoft.WebsiteCrawler.Utils.Helpers
{
	public static class StringHelper
	{
        private static Regex _regex = new Regex(@"\p{C}+", RegexOptions.Compiled);

        public static string Clean(this string source)
        {
            if (source == null)
                return null;
            return _regex.Replace(source, "");
        }

		public static string[] Split(this string source, string token)
		{
			return Regex.Split(source, token);
		}

		public static string Join(this IEnumerable<string> list, string separator)
		{
			var sb = new StringBuilder();

			foreach (var s in list)
			{
				if (sb.Length != 0)
					sb.Append(separator);
				sb.Append(s);
			}

			return sb.ToString();
		}

        public static string EmptyAsNull(this string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                value = null;

            return value;
        }

        public static bool Contains(this string value, string inner, StringComparison comparisonType)
        {
            return value.IndexOf(inner, comparisonType) >= 0;
        }

        public static string ExtractSurroundingText(this string text, int position, int lengthBefore, int lengthAfter)
        {
            string result = "...";
            int start = position - lengthBefore;
            int ending = position + lengthAfter;

            if (start < 0)
            {
                result = "";
                start = 0;
            }

            if (ending >= text.Length)
            {
                result += text.Substring(start);
            }
            else
            {
                result += text.Substring(start, ending - start) + "...";
            }

            return result;
        }

        public static string RemoveLineBreaks(this string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c == '\t')
                    sb.Append("    ");
                else if (!Char.IsControl(c) && c != '\r' && c != '\n')
                    sb.Append(c);
            }
            return sb.ToString();
        }
	}
}