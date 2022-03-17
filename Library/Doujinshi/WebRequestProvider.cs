using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons.Net;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class WebRequestProvider
    {
        public WebExplorer Explorer { get; } = null;
        public WebProxySettings Proxy { get; set; } = null;
        public int Timeout { get; set; } = 0;

        public WebRequestProvider() : this(new WebExplorer())
        {

        }

        public WebRequestProvider(WebExplorer explorer)
        {
            this.Explorer = explorer;
        }

        public virtual WebRequestParameter CreateRequestParameter()
        {
            var req = new WebRequestParameter { Proxy = this.Proxy };
            var timeout = this.Timeout;

            if (timeout > 0)
            {
                req.Timeout = timeout;
            }

            return req;
        }

    }

}
