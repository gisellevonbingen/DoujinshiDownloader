using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class StringUtils
    {
        public static string Substring(this string text, string prefix, string suffix)
        {
            if (text == null)
            {
                return null;
            }

            var s = text.IndexOf(prefix);

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

            return text.Substring(s, e - s);
        }

    }

}
