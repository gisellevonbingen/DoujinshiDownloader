using Giselle.DoujinshiDownloader.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
