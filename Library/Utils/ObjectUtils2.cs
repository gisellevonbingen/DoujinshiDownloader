using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class ObjectUtils2
    {
        public static T AddAndGet<T>(this ICollection<T> list, T value)
        {
            list.Add(value);

            return value;
        }

    }

}
