using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class StringUtils2
    {
        public static string Substring(this string text, string prefix, string suffix)
        {
            return Substring(text, prefix, suffix, 0);
        }

        public static string Substring(this string text, string prefix, string suffix, int startIndex)
        {
            return Substring(text, prefix, suffix, startIndex, out _);
        }

        public static string Substring(this string text, string prefix, string suffix, int startIndex, out int endIndex)
        {
            if (text == null)
            {
                endIndex = -1;
                return null;
            }

            int s = 0;
            var e = text.Length;

            if (string.IsNullOrWhiteSpace(prefix) == false)
            {
                s = text.IndexOf(prefix, startIndex);

                if (s == -1)
                {
                    throw new IndexOutOfRangeException();
                }

                s += prefix.Length;
            }

            if (string.IsNullOrWhiteSpace(suffix) == false)
            {
                e = text.IndexOf(suffix, s);

                if (e == -1)
                {
                    throw new IndexOutOfRangeException();
                }

            }

            endIndex = e;
            return text.Substring(s, e - s);
        }

    }

}
