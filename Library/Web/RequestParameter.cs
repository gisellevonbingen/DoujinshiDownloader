using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Web
{
    public class RequestParameter
    {
        public string URL { get; set; } = null;
        public string Method { get; set; } = null;
        public string Referer { get; set; } = null;
        public string ContentType { get; set; } = null;
        public string Accept { get; set; } = null;
        public CookieContainer CookieContainer { get; set; } = null;
        public int Timeout { get; set; } = 60 * 1000;
        public bool AllowAutoRedirect { get; set; } = false;
        public object WriteParameter { get; set; } = null;
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public ProxySettings Proxy { get; set; } = null;
        public bool KeepAlive { get; set; } = true;
    }

}
