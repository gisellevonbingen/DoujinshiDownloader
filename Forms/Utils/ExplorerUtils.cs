using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class ExplorerUtils
    {
        public static void Open(string path)
        {
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory) == true)
            {
                Process.Start("explorer", string.Concat("\"", path, "\""));
            }
            else
            {
                Process.Start("explorer", string.Concat("/select, \"", path, "\""));
            }

        }

    }

}
