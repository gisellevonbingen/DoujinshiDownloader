using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class FileSizeUtils
    {
        public const long F = 1024;

        public static string ToString(double size, int dps)
        {
            var units = EnumUtils.GetValues<FileSizeUnit>();
            int count = 0;

            while (true)
            {
                if (count + 1 >= units.Length || size < F)
                {
                    break;
                }

                size /= F;
                count++;
            }

            return size.ToString("F" + dps) + units[count].ToString();
        }

    }

}
