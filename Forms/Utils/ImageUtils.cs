using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class ImageUtils
    {
        public static Image FromBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            try
            {
                using (var ms = new MemoryStream(bytes))
                {
                    return Image.FromStream(ms);
                }

            }
            catch
            {
                return null;
            }

        }

    }

}
