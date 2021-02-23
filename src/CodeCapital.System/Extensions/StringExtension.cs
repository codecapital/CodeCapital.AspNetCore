using System;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeCapital.System.Extensions
{
    public static class StringExtension
    {
        public static string RemoveHtmlTag(this string text, string tag)
            => Regex.Replace(text, string.Format("<{0}.*?>.*?</{0}>", tag), string.Empty);

        public static string Clean(this string text)
            => Regex.Replace(text, @"[^\u0000-\u007F]", string.Empty);

        public static string SafeSubstring(this string? text, int length, bool isPostfix = false, bool isWholeWord = true, bool isHtml = false)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            if (!isHtml) text = Regex.Replace(text, "<.*?>", string.Empty);

            if (text.Length <= length) return text;

            text = text.Trim();

            if (isWholeWord && text.IndexOf(' ') < length)
            {
                return text.Substring(0, text.Substring(0, length).LastIndexOf(" ", StringComparison.Ordinal)) + (isPostfix ? ".." : "");
            }

            return text.Substring(0, length) + (isPostfix ? ".." : "");
        }

        public static string ParseHtml(this string text)
            => Regex.Match(text, "@{.*?}(.*)", RegexOptions.Singleline).Groups[1].Value;

        public static string Base64ForUrlDecode(this string str)
            => Encoding.UTF8.GetString(Convert.FromBase64String(str));
    }
}
