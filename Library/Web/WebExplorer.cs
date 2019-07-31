using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Commons;

namespace Giselle.DoujinshiDownloader.Web
{
    public class WebExplorer
    {
        static WebExplorer()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 9999;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
        }

        private static bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private CookieContainer _Cookies = null;
        public CookieContainer Cookies { get { return this._Cookies; } }

        public WebExplorer()
        {
            this._Cookies = new CookieContainer();
        }

        public HttpWebRequest CreateRequest(RequestParameter parameter)
        {
            string url = parameter.URL;
            Uri uri = new Uri(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            var headers = request.Headers;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = parameter.Method;
            request.ProtocolVersion = HttpVersion.Version11;
            request.Host = uri.Host;
            request.KeepAlive = parameter.KeepAlive;
            headers["Cache-Control"] = "max-age=0";
            headers["Upgrade-Insecure-Requests"] = "1";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            headers["DNT"] = "1";
            headers["Accept-Language"] = "ko-KR,ko;q=0.8,en-US;q=0.6,en;q=0.4";

            if (parameter.ContentType != null)
            {
                request.ContentType = parameter.ContentType;
            }

            if (parameter.Accept != null)
            {
                request.Accept = parameter.Accept;
            }

            foreach (var pair in parameter.Headers)
            {
                headers[pair.Key] = pair.Value;
            }

            CookieContainer cookies = new CookieContainer();
            cookies.Add(this.Cookies.GetCookies(uri));

            if (parameter.CookieContainer != null)
            {
                cookies.Add(parameter.CookieContainer.GetCookies(uri));
            }

            if (parameter.Referer != null)
            {
                request.Referer = parameter.Referer;
            }

            request.CookieContainer = cookies;
            request.AllowAutoRedirect = parameter.AllowAutoRedirect;

            var proxy = parameter.Proxy;

            if (proxy != null)
            {
                WebProxy proxyImpl = new WebProxy(proxy.Hostname, proxy.Port);
                proxyImpl.BypassProxyOnLocal = false;
                request.Proxy = proxyImpl;
            }

            int timeout = parameter.Timeout;
            request.ReadWriteTimeout = timeout;
            request.Timeout = timeout;
            request.ContinueTimeout = timeout;

            return request;
        }

        private void WriteReuqestParameter(HttpWebRequest request, object writeParameter)
        {
            if (writeParameter == null)
            {
                return;
            }

            Stream requestStream = null;

            try
            {
                requestStream = request.GetRequestStream();

                if (writeParameter is RequestWriteEventHandler)
                {
                    var handler = (RequestWriteEventHandler)writeParameter;
                    var e = new RequestWriteEventArgs(requestStream);
                    handler(this, e);
                }
                else if (writeParameter is byte[])
                {
                    byte[] value = (byte[])writeParameter;
                    requestStream.Write(value, 0, value.Length);
                }
                else
                {
                    byte[] value = Encoding.Default.GetBytes(string.Concat(writeParameter));
                    requestStream.Write(value, 0, value.Length);
                }

            }
            finally
            {
                ObjectUtils.DisposeQuietly(requestStream);
            }

        }

        public SessionResponse Request(RequestParameter parameter, CancellationToken? cancelToken = null)
        {
            HttpWebResponse response = null;
            Exception innerException = null;

            try
            {
                var request = this.CreateRequest(parameter);
                this.WriteReuqestParameter(request, parameter.WriteParameter);

                try
                {
                    cancelToken?.Register(() => request.Abort());
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException e)
                {
                    innerException = e;
                    response = (HttpWebResponse)e.Response;
                }

                if (response != null)
                {
                    this.Cookies.Add(response.Cookies);

                    return new SessionResponse(response);
                }

            }
            catch (Exception e)
            {
                innerException = e;
            }

            throw new NetworkException("", innerException);
        }

    }

}
