﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public abstract class FileArchive : IDisposable
    {
        public abstract string FilePath { get; }

        public abstract bool Exist { get; }

        public FileArchive()
        {

        }

        public abstract void Write(string fileName, byte[] bytes);

        ~FileArchive()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

        }

    }

}
