using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;

namespace Giselle.DoujinshiDownloader.Resources
{
    public class FontManager : IDisposable
    {
        private static readonly string _FontFamily = "맑은 고딕";
        public static string FontFamily { get { return _FontFamily; } }

        private readonly Dictionary<FontFormat, Font> FontMap = new Dictionary<FontFormat, Font>();

        public Font this[FontFormat format]
        {
            get
            {
                return this.GetFont(format);
            }

        }

        public Font this[float size, FontStyle style]
        {
            get
            {
                return this.GetFont(size, style);
            }

        }

        public FontManager()
        {

        }

        ~FontManager()
        {
            this.Dispose(false);
        }

        public Font GetFont(FontFormat format)
        {
            var map = this.FontMap;
            Font font = null;

            if (this.FontMap.ContainsKey(format))
            {
                font = map[format];
            }
            else
            {
                font = new Font(FontFamily, format.Size, format.Style);
                map[format] = font;
            }

            return font;
        }

        public Font GetFont(float size, FontStyle style)
        {
            return this.GetFont(new FontFormat(size, style));
        }

        public Font FindMatch(string text, FontMatchFormat format)
        {
            var proposedSize = format.ProposedSize;
            var size = format.Size;

            while (true)
            {
                if (size <= 1.0F)
                {
                    return this[1.0F, format.Style];
                }

                var font = this[size, format.Style];
                var textSize = TextRenderer.MeasureText(text, font, proposedSize, TextFormatFlags.WordEllipsis | TextFormatFlags.WordBreak | TextFormatFlags.PathEllipsis | TextFormatFlags.EndEllipsis);

                if (textSize.Height <= proposedSize.Height && textSize.Width <= proposedSize.Width)
                {
                    return font;
                }
                else
                {
                    size--;
                }

            }

        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                foreach (var font in this.FontMap.Values)
                {
                    ObjectUtils.DisposeQuietly(font);
                }

                this.FontMap.Clear();
            }

        }

    }

}
