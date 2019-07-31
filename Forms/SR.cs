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
        public static string Get(string name, Dictionary<string, string> args)
        {
            var text = DoujinshiDownloader.Instance.ResourceManager.GetString(name);
            return Replace(text, args);
        }

        public static string Get(string name, params string[] array)
        {
            var text = DoujinshiDownloader.Instance.ResourceManager.GetString(name);
            return Replace(text, array);
        }

        public static string Replace(string text, Dictionary<string, string> args)
        {
            foreach (var pair in args)
            {
                var key = pair.Key;
                var value = pair.Value;
                text = text.Replace($"{{={key}}}", value);
            }

            return text;
        }

        public static string Replace(string text, params string[] array)
        {
            for (int i = 0; i < array.Length; i += 2)
            {
                var key = array[i + 0];
                var value = array[i + 1];
                text = text.Replace($"{{={key}}}", value);
            }

            return text;
        }

    }

}
