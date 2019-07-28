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

        public static void Execute<T>(this T obj, Action<T> action)
        {
            if (obj != null)
            {
                action(obj);
            }

        }

        public static R Execute<T, R>(this T obj, Func<T, R> func)
        {
            if (obj != null)
            {
                return func(obj);
            }

            return default;
        }

    }

}
