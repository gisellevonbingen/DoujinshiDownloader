using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Resources
{
    public class FontMatchFormat
    {
        public Size ProposedSize { get; set; } = new Size();
        public FontStyle Style { get; set; } = FontStyle.Regular;
        public float Size { get; set; } = 0.0F;

        public FontMatchFormat()
        {

        }

    }

}
