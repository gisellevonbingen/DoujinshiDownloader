using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons;
using HtmlAgilityPack;

namespace Giselle.DoujinshiDownloader.Web
{
    public class SessionResponse : IDisposable
    {
        private HttpWebResponse _Impl = null;
        public HttpWebResponse Impl { get { return this._Impl; } }

        public SessionResponse(HttpWebResponse impl)
        {
            this._Impl = impl;
        }

        ~SessionResponse()
        {
            this.Dispose(false);
        }

        public HtmlDocument ReadToDocument()
        {
            string html = this.ReadToEnd();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            return document;
        }

        public string ReadToEnd()
        {
            Stream stream = null;
            StreamReader reader = null;

            try
            {
                stream = this.Impl.GetResponseStream();
                reader = new StreamReader(stream);

                return reader.ReadToEnd();
            }
            finally
            {
                ObjectUtils.DisposeQuietly(reader);
                ObjectUtils.DisposeQuietly(stream);
            }

        }

        public Stream ReadToStream()
        {
            return this.Impl.GetResponseStream();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            ObjectUtils.DisposeQuietly(this.Impl);
        }

    }

}