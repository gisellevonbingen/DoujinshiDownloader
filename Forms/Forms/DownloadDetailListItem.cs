using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.DoujinshiDownloader.Schedulers;
using Giselle.DoujinshiDownloader.Utils;

namespace Giselle.DoujinshiDownloader.Forms
{
    public class DownloadDetailListItem : OptimizedControl
    {
        private DownloadResult _State = DownloadResult.StandBy;
        public event EventHandler StateChanged = null;

        public DownloadDetailListItem()
        {

        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
        }

        public DownloadResult State
        {
            get
            {
                return this._State;
            }

            set
            {
                this._State = value;
                this.OnStateChanged(EventArgs.Empty);
            }

        }

        private void OnStateChanged(EventArgs e)
        {
            this.StateChanged?.Invoke(this, e);

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var font = this.Font;
            var text = this.Text;
            var state = this.State;
            var bounds = this.DisplayRectangle;

            using (var brush = new SolidBrush(this.ForeColor))
            {
                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Center;
                    g.DrawString(text + " : " + SR.Get($"Download.Detail.State.{state.ToString()}"), font, brush, bounds, format);
                }

            }

        }

    }

}
