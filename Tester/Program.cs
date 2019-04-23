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
            var url = "//g.hitomi.la/galleries/1402996/253.png";
            var matches = Regex.Matches(url, @"\/\d*(\d)\/");

            foreach (var match in matches)
            {
                Console.WriteLine(match);
            }

            return;
        }

    }

}
