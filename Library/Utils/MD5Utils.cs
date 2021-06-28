using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Utils
{
    public static class MD5Utils
    {
        public static string GetMD5String(this string value)
        {
            return GetMD5String(value, Encoding.Default);
        }

        public static string GetMD5String(this string value, Encoding encoding)
        {
            using (var md5 = MD5.Create())
            {
                var source = encoding.GetBytes(value);
                var output = md5.ComputeHash(source);
                return BitConverter.ToString(output).Replace("-", string.Empty);
            }

        }

    }

}
