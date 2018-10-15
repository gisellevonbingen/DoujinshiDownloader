using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class OptimizedGroupBox : OptimizedControl
    {
        public OptimizedGroupBox()
        {
            this.SuspendLayout();

            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

            this.Font = fm[12, FontStyle.Regular];

            this.Padding = new Padding(10, 27, 10, 10);

            this.ResumeLayout(false);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var text = this.Text;
            var font = this.Font;
            var size = this.ClientSize;
            var g = e.Graphics;
            var textSize = g.MeasureString(text, font);
            var textLocation = new PointF(10, 0);
            var textBounds = new RectangleF(textLocation, textSize);

            using (var brush = new SolidBrush(Color.Black))
            {
                g.DrawString(text, font, brush, textBounds);
            }

            using (var pen = new Pen(Color.Black, 1))
            {
                int lo = 1;
                var lineTop = textSize.Height / 2;
                var lineOutBounds = RectangleF.FromLTRB(0, lineTop, size.Width - lo, size.Height - lo);

                var points = new List<PointF>();
                points.Add(new PointF(textBounds.Left, lineOutBounds.Top));
                points.Add(new PointF(lineOutBounds.Left, lineOutBounds.Top));
                points.Add(new PointF(lineOutBounds.Left, lineOutBounds.Bottom));
                points.Add(new PointF(lineOutBounds.Right, lineOutBounds.Bottom));
                points.Add(new PointF(lineOutBounds.Right, lineOutBounds.Top));
                points.Add(new PointF(textBounds.Right, lineOutBounds.Top));
                g.DrawLines(pen, points.ToArray());
            }

        }

    }

}
