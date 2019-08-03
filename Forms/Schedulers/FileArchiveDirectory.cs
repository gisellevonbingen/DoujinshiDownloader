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

        public override void Write(string fileName, byte[] bytes)
        {
            using (var localStream = new FileStream(Path.Combine(this.Directory, fileName), FileMode.Create))
            {
                localStream.Write(bytes, 0, bytes.Length);
            }

        }

    }

}
