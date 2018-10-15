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
        private Size _ProposedSize = new Size();
        public Size ProposedSize { get { return this._ProposedSize; } set { this._ProposedSize = value; } }

        private FontStyle _Style = FontStyle.Regular;
        public FontStyle Style { get { return this._Style; } set { this._Style = value; } }

        private float _Size = 0.0F;
        public float Size { get { return this._Size; } set { this._Size = value; } }

        public FontMatchFormat()
        {

        }

    }

}
