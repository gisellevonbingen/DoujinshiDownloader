using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class GalleryParameterValues
    {
        private readonly Dictionary<IGalleryParameterType, object> Values;

        public GalleryParameterValues()
        {
            this.Values = new Dictionary<IGalleryParameterType, object>();
        }

        public void Set<T>(GalleryParameterType<T> type, T value)
        {
            if (type.Available(value) == true)
            {
                var values = this.Values;

                lock (values)
                {
                    values[type] = value;
                }

            }

        }

        public T Get<T>(GalleryParameterType<T> type, T fallback)
        {
            var values = this.Values;

            lock (values)
            {
                if (values.TryGetValue(type, out var value) == true && value is T t)
                {
                    return t;
                }
                else
                {
                    return fallback;
                }

            }

        }

    }

}
