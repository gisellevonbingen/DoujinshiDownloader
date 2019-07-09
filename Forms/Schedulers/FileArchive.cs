using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public abstract class FileArchive : IDisposable
    {
        public abstract string FilePath { get; }

        public FileArchive()
        {

        }

        public abstract void Write(string fileName, Stream stream);

        ~FileArchive()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {

        }

    }

}
