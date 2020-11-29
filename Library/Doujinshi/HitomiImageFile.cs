using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class HitomiImageFile
    {
        public bool HasAvif { get; set; }
        public bool HasAvifSmalltn { get; set; }
        public bool HasWebp { get; set; }
        public string Hash { get; set; }
        public string Name { get; set; }

        public HitomiImageFile()
        {

        }

    }

}
