using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class UriUtils
    {
        public static string GetFileName(this Uri uri)
        {
            var fileName = uri.LocalPath;
            int slashIndex = fileName.LastIndexOf('/');

            if (slashIndex > -1)
            {
                fileName = fileName.Substring(slashIndex + 1);
            }

            return fileName;
        }

    }

}
