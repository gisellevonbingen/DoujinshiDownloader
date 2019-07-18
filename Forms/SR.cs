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
        public static string Get(string name, params string[] array)
        {
            var text = DoujinshiDownloader.Instance.ResourceManager.GetString(name);

            for (int i = 0; i < array.Length; i+= 2)
            {
                var key = array[i + 0];
                var value = array[i + 1];
                text = text.Replace($"{{={key}}}", value);

            }

            return text;
        }

    }

}
