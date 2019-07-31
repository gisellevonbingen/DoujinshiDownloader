using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class StringUtils
    {
        public static string[] Cut(this string text, params string[] keywords)
        {
            var startIndex = 0;
            var result = new string[keywords.Length - 1];

            for (int i = 0; i < keywords.Length - 1;i ++)
            {
                var prev = keywords[i + 0];
                var next = keywords[i + 1];

                result[i] = Substring(text, prev, next, startIndex, out startIndex);
            }

            return result;
        }

        public static string Substring(this string text, string prefix, string suffix)
        {
            return Substring(text, prefix, suffix, 0);
        }

        public static string Substring(this string text, string prefix, string suffix, int startIndex)
        {
            return Substring(text, prefix, suffix, 0, out var endIndex);
        }

        public static string Substring(this string text, string prefix, string suffix, int startIndex, out int endIndex)
        {
            if (text == null)
            {
                endIndex = -1;
                return null;
            }

            var s = text.IndexOf(prefix, startIndex);

            if (s == -1)
            {
                throw new IndexOutOfRangeException();
            }

            s += prefix.Length;

            var e = text.IndexOf(suffix, s);

            if (e == -1)
            {
                throw new IndexOutOfRangeException();
            }

            endIndex = e;
            return text.Substring(s, e - s);
        }

    }

}
