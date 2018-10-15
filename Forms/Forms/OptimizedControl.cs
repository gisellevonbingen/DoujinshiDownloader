using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class OptimizedControl : Control
    {
        public OptimizedControl()
        {
            this.SuspendLayout();

            var dd = DoujinshiDownloader.Instance;
            var fm = dd.FontManager;

            this.Font = fm[11, FontStyle.Regular];

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.StandardDoubleClick, false);
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            this.ResumeLayout(false);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            var map = this.GetPreferredBounds(proposedSize);
            var offset = this.GetPreferredSizeOffset();
            int width = 0;
            int height = 0;

            foreach (var bounds in map.Values)
            {
                width = Math.Max(width, bounds.Right);
                height = Math.Max(height, bounds.Bottom);
            }

            return new Size(width + offset.Width, height + offset.Height);
        }

        protected virtual Size GetPreferredSizeOffset()
        {
            var padding = this.Padding;
            return new Size(padding.Right, padding.Bottom);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            this.UpdateControlsBoundsPreferred();
        }

        protected virtual void UpdateControlsBoundsPreferred()
        {
            this.UpdateControlsBoundsPreferred(this.ClientSize);
        }

        protected virtual void UpdateControlsBoundsPreferred(Size size)
        {
            var map = this.GetPreferredBounds(size);

            foreach (var pair in map)
            {
                var control = pair.Key;
                var bounds = pair.Value;

                if (control != null)
                {
                    control.Bounds = bounds;
                }

            }

        }

        public virtual Dictionary<Control, Rectangle> GetPreferredBounds(Size size)
        {
            var layoutBounds = this.GetLayoutBounds(size);
            var map = this.GetPreferredBounds(layoutBounds);

            return map;
        }

        protected virtual Rectangle GetLayoutBounds(Size size)
        {
            var padding = this.Padding;

            return Rectangle.FromLTRB(padding.Left, padding.Top, size.Width - padding.Right, size.Height - padding.Bottom);
        }

        protected virtual Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = new Dictionary<Control, Rectangle>();

            return map;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                foreach (var control in this.Controls.Cast<Control>())
                {
                    ObjectUtils.DisposeQuietly(control);
                }

                this.Controls.Clear();
            }

        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            this.Focus();
        }

        protected override void OnNotifyMessage(Message m)
        {
            if (m.Msg == (int)WindowsMessage.WM_ERASEBKGND)
            {
                return;
            }

            base.OnNotifyMessage(m);
        }

    }

}