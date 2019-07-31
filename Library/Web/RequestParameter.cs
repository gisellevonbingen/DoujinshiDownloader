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
        private string _URL = null;
        public string URL { get { return this._URL; } set { this._URL = value; } }

        private string _Method = null;
        public string Method { get { return this._Method; } set { this._Method = value; } }

        private string _Referer = null;
        public string Referer { get { return this._Referer; } set { this._Referer = value; } }

        private string _ContentType = null;
        public string ContentType { get { return this._ContentType; } set { this._ContentType = value; } }

        private string _Accept = null;
        public string Accept { get { return this._Accept; } set { this._Accept = value; } }

        private CookieContainer _CookieContainer = null;
        public CookieContainer CookieContainer { get { return this._CookieContainer; } set { this._CookieContainer = value; } }

        private int _Timeout = 60 * 1000;
        public int Timeout { get { return this._Timeout; } set { this._Timeout = value; } }

        private bool _AllowAutoRedirect = false;
        public bool AllowAutoRedirect { get { return this._AllowAutoRedirect; } set { this._AllowAutoRedirect = value; } }

        private object _WriteParameter = null;
        public object WriteParameter { get { return this._WriteParameter; } set { this._WriteParameter = value; } }

        private Dictionary<string, string> _Headers = new Dictionary<string, string>();
        public Dictionary<string, string> Headers { get { return this._Headers; } }

        private ProxySettings _Proxy = null;
        public ProxySettings Proxy { get { return this._Proxy; } set { this._Proxy = value; } }

        private bool _KeepAlive = true;
        public bool KeepAlive { get { return this._KeepAlive; } set { this._KeepAlive = value; } }

        public RequestParameter()
        {

        }

    }

}
