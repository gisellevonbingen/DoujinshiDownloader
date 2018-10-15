using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Web
{
    public delegate void RequestWriteEventHandler(object sender, RequestWriteEventArgs e);

    public class RequestWriteEventArgs : EventArgs
    {
        private Stream _Stream = null;
        public Stream Stream { get { return this._Stream; } }

        public RequestWriteEventArgs(Stream stream)
        {
            this._Stream = stream;
        }

    }

}
