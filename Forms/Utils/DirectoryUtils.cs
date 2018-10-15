using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class DirectoryUtils
    {
        public static bool IsValidate(string path)
        {
            bool result = false;

            try
            {
                var realPath = Path.Combine(Directory.GetCurrentDirectory(), path);
                var di = new DirectoryInfo(realPath);

                if (di.Exists == false)
                {
                    di.Create();
                    di.Delete();
                }

                result = true;
            }
            catch
            {

            }

            return result;
        }

    }

}
