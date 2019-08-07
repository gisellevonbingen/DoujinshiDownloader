using Giselle.DoujinshiDownloader.Doujinshi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tester
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var list = new List<string>();
            list.Add("1426111");
            list.Add("1426111/f31e10992f");
            list.Add("https://e-hentai.org/g/1426111/f31e10992f");
            list.Add("https://exhentai.org/g/1426111/f31e10992f");
            list.Add("https://e-hentai.org/g/1426111/f31e10992f/");
            list.Add("https://exhentai.org/g/1426111/f31e10992f/");
            list.Add("https://hitomi.la/galleries/1445130.html");
            list.Add("https://hitomi.la/reader/1262768.html");
            list.Add("https://hitomi.la/reader/1262768.html#1-");

            foreach (var str in list)
            {
                var downloadInput = DownloadInput.Parse(str);
                Console.WriteLine("=================");
                Console.WriteLine(str);
                Console.WriteLine(downloadInput);
                Console.WriteLine();
            }

            return;
        }

    }

}
