using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Schedulers
{
    public class FileArchiveDirectory : FileArchive
    {
        public string Directory { get; }

        public FileArchiveDirectory(string directory)
        {
            this.Directory = directory;
            System.IO.Directory.CreateDirectory(directory);
        }

        public override string FilePath => this.Directory;

        public override void Write(string fileName, Stream stream)
        {
            using (var localStream = new FileStream(Path.Combine(this.Directory, fileName), FileMode.Create))
            {
                stream.CopyTo(localStream);
            }

        }

    }

}
