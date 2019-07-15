using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class ListItemWrapper<T>
    {
        public string Name { get; }
        public T Value { get; }

        public ListItemWrapper(string name, T value)
        {
            this.Name = name;
            this.Value = value;
        }

        public override string ToString()
        {
            return this.Name;
        }

    }

}
