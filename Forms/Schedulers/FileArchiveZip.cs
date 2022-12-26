using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons;
using ICSharpCode.SharpZipLib.Zip;

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

        public override bool Exist => File.Exists(this.FilePath);

        public override void Write(string fileName, byte[] bytes)
        {
            var zipEntry = new ZipEntry(fileName);
            this.Stream.PutNextEntry(zipEntry);
            this.Stream.Write(bytes, 0, bytes.Length);
            this.Stream.CloseEntry();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.Stream.DisposeQuietly();
            this.BaseStream.DisposeQuietly();
        }

    }

}
