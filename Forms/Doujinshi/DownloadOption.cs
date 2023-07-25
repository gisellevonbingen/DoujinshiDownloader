using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons.Collections;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public interface IDownloadOption : INamed
    {
        Type ValueType { get; }
        object DefaultValue { get; }
    }

    public class DownloadOption<VALUETYPE> : IDownloadOption
    {
        public string Name { get; }
        public Type ValueType { get; }
        public VALUETYPE DefaultValue { get; }
        object IDownloadOption.DefaultValue { get => this.DefaultValue; }

        public DownloadOption(string name, VALUETYPE defaultValue)
        {
            this.Name = name;
            this.ValueType = typeof(VALUETYPE);
            this.DefaultValue = defaultValue;
        }

        public VALUETYPE GetValue(Dictionary<IDownloadOption, object> options) => options.TryGetValue(this, out var raw) ? (VALUETYPE)raw : this.DefaultValue;
    }

}
