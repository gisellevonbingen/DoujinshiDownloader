using Giselle.Commons;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class FileArchiveZip : FileArchive
    {
        public override string FilePath { get; }
        public Stream BaseStream { get; }
        public ZipOutputStream Stream { get; }

        public FileArchiveZip(string filePath)
        {
            this.FilePath = filePath;
            this.BaseStream = new FileStream(filePath, FileMode.Create);
            this.Stream = new ZipOutputStream(this.BaseStream);
        }

        public override void Write(string fileName, Stream stream)
        {
            var zipEntry = new ZipEntry(fileName);
            this.Stream.PutNextEntry(zipEntry);
            stream.CopyTo(this.Stream);
            this.Stream.CloseEntry();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            ObjectUtils.DisposeQuietly(this.Stream);
            ObjectUtils.DisposeQuietly(this.BaseStream);
        }

    }

}
