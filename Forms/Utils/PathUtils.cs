using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class PathUtils
    {
        public static string GetFilePathNotDuplicate(string path)
        {
            if (File.Exists(path) == true)
            {
                var extension = Path.GetExtension(path);
                var withoutExtention = Path.ChangeExtension(path, null);

                for (int i = 1; ; i++)
                {
                    path = Path.ChangeExtension(withoutExtention + "_" + i, extension);

                    if (File.Exists(path) == false)
                    {
                        return path;
                    }

                }

            }
            else
            {
                return path;
            }

        }

        public static string GetPath(params string[] paths)
        {
            var list = new List<string>();
            list.Add(Application.StartupPath);
            list.AddRange(paths);
            return Path.Combine(list.ToArray());
        }

        public static string FilterInvalids(string fileName)
        {
            var builder = new StringBuilder();
            var invalidChars = Path.GetInvalidFileNameChars();

            foreach (var c in fileName)
            {
                if (Array.IndexOf(invalidChars, c) == -1)
                {
                    builder.Append(c);
                }

            }

            return builder.ToString();
        }

    }

}
