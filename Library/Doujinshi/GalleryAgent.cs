using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Web;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public abstract class GalleryAgent
    {
        private WebExplorer _Explorer = null;
        protected WebExplorer Explorer { get { return this._Explorer; } }

        private ProxySettings _Proxy = null;
        public ProxySettings Proxy { get { return this._Proxy; } set { this._Proxy = value; } }

        public GalleryAgent()
        {
            this._Explorer = new WebExplorer();
        }

        public virtual RequestParameter CreateRequestParameter()
        {
            var requeset = new RequestParameter();
            requeset.Proxy = this.Proxy;

            return requeset;
        }

        public abstract string GetGalleryTitle(string url, DownloadAgentParameter agentParameter);

        public abstract List<string> GetGalleryImageViewURLs(string url);

        public virtual DownloadGalleryParameter CreateGalleryParameter(string url)
        {
            return new DownloadGalleryParameter { Referer = url };
        }

        public abstract RequestParameter GetGalleryImageDownloadRequest(string url, DownloadGalleryParameter galleryParameter, DownloadAgentParameter agentParameter);

        public void Download(RequestParameter parameter, string localDirectory, byte[] buffer)
        {
            SessionResponse response = null;
            Stream responseStream = null;
            Stream localStream = null;

            try
            {
                response = this.Explorer.Request(parameter);
                responseStream = response.ReadToStream();
                long length = response.Impl.ContentLength;
                long position = 0L;

                string fileName = new Uri(parameter.URL).LocalPath;
                int slashIndex = fileName.LastIndexOf('/');

                if (slashIndex > -1)
                {
                    fileName = fileName.Substring(slashIndex + 1);
                }

                localStream = new FileStream(Path.Combine(localDirectory, fileName), FileMode.Create);

                for (int len = 0; (len = responseStream.Read(buffer, 0, buffer.Length)) > 0;)
                {
                    localStream.Write(buffer, 0, len);
                    position += len;
                }

            }
            catch (WebException e)
            {
                throw new NetworkException("", e);
            }
            finally
            {
                ObjectUtils.CloseAndDisposeQuietly(localStream);
                ObjectUtils.CloseAndDisposeQuietly(responseStream);
                ObjectUtils.CloseAndDisposeQuietly(response);
            }

        }

    }

}
