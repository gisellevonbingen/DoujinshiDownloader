using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader
{
    public static class SR
    {
        public static string Get(string name)
        {
            return DoujinshiDownloader.Instance.ResourceManager.GetString(name);
        }

    }

}
