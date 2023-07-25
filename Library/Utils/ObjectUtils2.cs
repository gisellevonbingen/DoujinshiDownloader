using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class ObjectUtils2
    {
        public static T2 AddAndGet<T, T2>(this ICollection<T> list, T2 value) where T2 : T
        {
            list.Add(value);

            return value;
        }

    }

}
