using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class OptimizedProgressBar : OptimizedControl
    {
        private Color _BarColor = Color.FromArgb(23, 168, 95);
        public Color BarColor { get { return this._BarColor; } set { this._BarColor = value; this.OnBarColorChanged(EventArgs.Empty); } }
        public event EventHandler BarColorChanged = null;

        private int _Minimum = 0;
        public int Minimum { get { return this._Minimum; } set { this._Minimum = value; this.OnMinimumChanged(EventArgs.Empty); } }
        public event EventHandler MinimumChanged = null;

        private int _Maximum = 0;
        public int Maximum { get { return this._Maximum; } set { this._Maximum = value; this.OnMaximumChanged(EventArgs.Empty); } }
        public event EventHandler MaximumChanged = null;

        private int _Value = 0;
        public int Value { get { return this._Value; } set { this._Value = value; this.OnValueChanged(EventArgs.Empty); } }
        public event EventHandler ValueChanged = null;

        public OptimizedProgressBar()
        {
            this.SuspendLayout();

            this.Padding = new Padding(1);
            this.BackColor = Color.FromKnownColor(KnownColor.Control);
            this.ForeColor = Color.Black;

            this.ResumeLayout(false);
        }

        private void OnValueChanged(EventArgs e)
        {
            this.ValueChanged?.Invoke(this, e);

            this.Invalidate();
        }

        private void OnMaximumChanged(EventArgs e)
        {
            this.MaximumChanged?.Invoke(this, e);

            this.Invalidate();
        }

        private void OnMinimumChanged(EventArgs e)
        {
            this.MinimumChanged?.Invoke(this, e);

            this.Invalidate();
        }

        private void OnBarColorChanged(EventArgs e)
        {
            this.BarColorChanged.Invoke(this, e);

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var padding = this.Padding;
            var size = this.ClientSize;

            var barBounds = Rectangle.FromLTRB(padding.Left, padding.Top, size.Width - padding.Right, size.Height - padding.Bottom);

            using (var backBrush = new SolidBrush(this.BackColor))
            {
                g.Clear(Color.Black);

                g.FillRectangle(backBrush, barBounds);
            }

            var value = this.Value;
            var minimum = this.Minimum;
            var maximum = this.Maximum;

            var valuePercent = (double)(value - minimum) / (double)(maximum - minimum);
            var valueBounds = new Rectangle(barBounds.Left, barBounds.Top, (int)(barBounds.Width * valuePercent), barBounds.Height);

            using (var barBrush = new SolidBrush(this.BarColor))
            {
                g.FillRectangle(barBrush, valueBounds);
            }

            using (var foreBrush = new SolidBrush(this.ForeColor))
            {
                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    g.DrawString(this.Text, this.Font, foreBrush, barBounds, format);
                }

            }

        }

    }

}
